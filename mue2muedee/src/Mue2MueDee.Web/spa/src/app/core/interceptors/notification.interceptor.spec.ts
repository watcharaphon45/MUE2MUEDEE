import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { LoadingService } from '../loading/loading.service';
import { NotificationService } from '../notifications/notification.service';
import { loadingInterceptor } from './loading.interceptor';
import { notificationInterceptor } from './notification.interceptor';

describe('API action notifications', () => {
  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({ providers: [
      provideHttpClient(withInterceptors([loadingInterceptor, notificationInterceptor])), provideHttpClientTesting()
    ] });
  });
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('does not announce successful reads and uses the API message after an action', () => {
    const http = TestBed.inject(HttpClient);
    const controller = TestBed.inject(HttpTestingController);
    const notifications = TestBed.inject(NotificationService);
    http.get('/api/items').subscribe();
    controller.expectOne('/api/items').flush({ success: true, message: 'ดึงข้อมูลแล้ว', data: [] });
    TestBed.tick();
    expect(notifications.items()).toEqual([]);
    http.post('/api/items', {}).subscribe();
    controller.expectOne('/api/items').flush({ success: true, message: 'บันทึกสินค้าสำเร็จ', data: {} });
    expect(notifications.items()).toEqual([]);
    TestBed.tick();
    expect(TestBed.inject(LoadingService).isLoading()).toBe(false);
    expect(notifications.items()[0]).toMatchObject({ kind: 'success', message: 'บันทึกสินค้าสำเร็จ' });
  });

  it('reports an approval requirement as a warning even on a successful response', () => {
    TestBed.inject(HttpClient).post('/api/purchases/po-1/offer', {}).subscribe();
    TestBed.inject(HttpTestingController).expectOne('/api/purchases/po-1/offer').flush({
      success: true, message: 'ราคาเกินเกณฑ์ ต้องขออนุมัติ', data: { needsApproval: true }
    });
    TestBed.tick();
    expect(TestBed.inject(NotificationService).items()[0]).toMatchObject({ kind: 'warning', message: 'ราคาเกินเกณฑ์ ต้องขออนุมัติ' });
  });

  it.each([400, 401, 403, 404, 409, 422, 429])('shows HTTP %s conditions without swallowing the error', status => {
    const failed = vi.fn();
    TestBed.inject(HttpClient).delete('/api/items/1').subscribe({ error: failed });
    TestBed.inject(HttpTestingController).expectOne('/api/items/1').flush({ message: 'ไม่สามารถทำรายการนี้ได้' }, { status, statusText: 'Rejected' });
    TestBed.tick();
    expect(failed).toHaveBeenCalledOnce();
    expect(TestBed.inject(NotificationService).items()[0]).toMatchObject({ kind: 'warning', message: 'ไม่สามารถทำรายการนี้ได้' });
  });

  it('deduplicates concurrent server errors and releases loading before showing the message', () => {
    const http = TestBed.inject(HttpClient);
    http.get('/api/first').subscribe({ error: () => {} });
    http.get('/api/second').subscribe({ error: () => {} });
    const controller = TestBed.inject(HttpTestingController);
    controller.expectOne('/api/first').flush({ message: 'ระบบขัดข้อง' }, { status: 500, statusText: 'Error' });
    TestBed.tick();
    expect(TestBed.inject(NotificationService).items()).toEqual([]);
    controller.expectOne('/api/second').flush({ message: 'ระบบขัดข้อง' }, { status: 500, statusText: 'Error' });
    TestBed.tick();
    expect(TestBed.inject(NotificationService).items()).toHaveLength(1);
    expect(TestBed.inject(NotificationService).items()[0].kind).toBe('error');
  });

  it('shows a connection error without exposing an HTTP implementation message', () => {
    TestBed.inject(HttpClient).get('/api/items').subscribe({ error: () => {} });
    TestBed.inject(HttpTestingController).expectOne('/api/items').error(new ProgressEvent('error'));
    TestBed.tick();
    expect(TestBed.inject(NotificationService).items()[0]).toMatchObject({ kind: 'error' });
    expect(TestBed.inject(NotificationService).items()[0].message).toContain('ไม่สามารถเชื่อมต่อเซิร์ฟเวอร์');
  });

  it('does not announce background session renewal', () => {
    TestBed.inject(HttpClient).post('/api/auth/refresh', {}).subscribe();
    TestBed.inject(HttpTestingController).expectOne('/api/auth/refresh').flush({ success: true, message: 'ต่ออายุเซสชันแล้ว' });
    TestBed.tick();
    expect(TestBed.inject(NotificationService).items()).toEqual([]);
  });
});
