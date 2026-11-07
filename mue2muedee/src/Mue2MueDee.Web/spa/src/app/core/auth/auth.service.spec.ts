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

  it('keeps Super Admin in platform mode until the server confirms a selected store', () => {
    TestBed.configureTestingModule({ providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()] });
    const service = TestBed.inject(AuthService);
    const http = TestBed.inject(HttpTestingController);
    const stores = [{ storeId: 'shop-1', storeName: 'Shop 1', storeCode: 'ONE', role: 'Owner' }];
    const response = { accessToken: 'platform-token', refreshToken: 'refresh-1',
      user: { id: 'admin', username: 'platform.admin', isSuperAdmin: true }, availableStores: stores };
    service.login('platform.admin', 'test').subscribe();
    http.expectOne('/api/auth/login').flush({ data: response });
    expect(service.availableStores().length).toBe(1);
    expect(service.isSuperAdmin()).toBe(true);
    expect(service.currentStore()).toBeNull();
    expect(localStorage.getItem('mue2_current_store')).toBeNull();
    service.switchStore('shop-1').subscribe();
    http.expectOne('/api/auth/switch-store').flush({ data: { ...response, accessToken: 'shop-token', currentStoreId: 'shop-1' } });
    expect(service.currentStore()?.storeId).toBe('shop-1');
    expect(service.accessToken()).toBe('shop-token');
    expect(service.isOwner()).toBe(true);
    http.verify();
  });
});
