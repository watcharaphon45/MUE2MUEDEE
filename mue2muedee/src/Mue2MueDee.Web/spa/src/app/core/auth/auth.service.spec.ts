import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from './auth.service';

describe('AuthService store context', () => {
  afterEach(() => localStorage.clear());

  it('clears a previously stored shop when a new session has no available shop', () => {
    localStorage.setItem('mue2_current_store', JSON.stringify({ storeId: 'old-shop', role: 'Owner' }));
    TestBed.configureTestingModule({ providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()] });
    const service = TestBed.inject(AuthService);
    const http = TestBed.inject(HttpTestingController);
    service.login('new-user', 'test').subscribe();
    http.expectOne('/api/auth/login').flush({ data: {
      accessToken: 'test-token', refreshToken: 'test-refresh',
      user: { id: 'new-user', username: 'new-user' }, availableStores: []
    } });
    expect(service.currentStore()).toBeNull();
    expect(service.currentRole()).toBeNull();
    expect(localStorage.getItem('mue2_current_store')).toBeNull();
    http.verify();
  });
});
