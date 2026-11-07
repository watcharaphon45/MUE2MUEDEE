import { TestBed } from '@angular/core/testing';
import { HttpClient, HttpEventType, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { retry, switchMap } from 'rxjs';
import { LoadingService } from '../loading/loading.service';
import { loadingInterceptor } from './loading.interceptor';

describe('Global request loading', () => {
  beforeEach(() => TestBed.configureTestingModule({ providers: [
    provideHttpClient(withInterceptors([loadingInterceptor])), provideHttpClientTesting()
  ] }));
  afterEach(() => TestBed.inject(HttpTestingController).verify({ ignoreCancelled: true }));

  it('starts on subscription and ends on a successful response', () => {
    const loading = TestBed.inject(LoadingService);
    const request = TestBed.inject(HttpClient).get('/api/items');
    expect(loading.pendingRequests()).toBe(0);
    request.subscribe();
    expect(loading.isLoading()).toBe(true);
    TestBed.inject(HttpTestingController).expectOne('/api/items').flush([]);
    expect(loading.isLoading()).toBe(false);
  });

  it('keeps the screen blocked until every concurrent request finishes, including errors', () => {
    const loading = TestBed.inject(LoadingService);
    const http = TestBed.inject(HttpClient);
    http.get('/api/items').subscribe({ error: () => {} });
    http.post('/api/action', {}).subscribe();
    expect(loading.pendingRequests()).toBe(2);
    const controller = TestBed.inject(HttpTestingController);
    controller.expectOne('/api/items').flush({}, { status: 500, statusText: 'Error' });
    expect(loading.pendingRequests()).toBe(1);
    expect(loading.isLoading()).toBe(true);
    controller.expectOne('/api/action').flush({});
    expect(loading.pendingRequests()).toBe(0);
  });

  it('releases a cancelled request', () => {
    const loading = TestBed.inject(LoadingService);
    const subscription = TestBed.inject(HttpClient).get('/api/items').subscribe();
    const request = TestBed.inject(HttpTestingController).expectOne('/api/items');
    subscription.unsubscribe();
    expect(request.cancelled).toBe(true);
    expect(loading.isLoading()).toBe(false);
  });

  it('keeps loading through upload progress until the response arrives', () => {
    const loading = TestBed.inject(LoadingService);
    TestBed.inject(HttpClient).post('/api/upload', new FormData(), { reportProgress: true, observe: 'events' }).subscribe();
    const request = TestBed.inject(HttpTestingController).expectOne('/api/upload');
    request.event({ type: HttpEventType.UploadProgress, loaded: 50, total: 100 });
    expect(loading.isLoading()).toBe(true);
    request.flush({ url: '/image.png' });
    expect(loading.isLoading()).toBe(false);
  });

  it('counts retries without leaving an unfinished request', () => {
    const loading = TestBed.inject(LoadingService);
    TestBed.inject(HttpClient).get('/api/items').pipe(retry(1)).subscribe();
    const controller = TestBed.inject(HttpTestingController);
    controller.expectOne('/api/items').flush({}, { status: 503, statusText: 'Unavailable' });
    expect(loading.pendingRequests()).toBe(1);
    controller.expectOne('/api/items').flush([]);
    expect(loading.pendingRequests()).toBe(0);
  });

  it('stays blocked between saving an action and reloading its data', () => {
    const loading = TestBed.inject(LoadingService);
    const http = TestBed.inject(HttpClient);
    http.post('/api/action', {}).pipe(switchMap(() => http.get('/api/items'))).subscribe();
    const controller = TestBed.inject(HttpTestingController);
    controller.expectOne('/api/action').flush({});
    expect(loading.pendingRequests()).toBe(1);
    expect(loading.isLoading()).toBe(true);
    controller.expectOne('/api/items').flush([]);
    expect(loading.isLoading()).toBe(false);
  });

  it('makes each release function idempotent', () => {
    const loading = TestBed.inject(LoadingService);
    const first = loading.begin();
    const second = loading.begin();
    first(); first();
    expect(loading.pendingRequests()).toBe(1);
    second();
    expect(loading.pendingRequests()).toBe(0);
  });
});
