import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, provideRouter, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { AuthService } from '../auth/auth.service';
import { storeContextGuard } from './auth.guard';

describe('Store route context', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({ providers: [provideRouter([]), provideHttpClient()] });
    const auth = TestBed.inject(AuthService);
    auth.accessToken.set('test-token');
    auth.currentUser.set({ id: 'admin', username: 'admin', email: 'admin@example.test', fullName: 'Admin', isSuperAdmin: true });
  });
  afterEach(() => localStorage.clear());
  const check = (url: string) => TestBed.runInInjectionContext(() =>
    storeContextGuard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot));

  it('allows platform management without a store and redirects store screens to platform management', () => {
    expect(check('/super-admin/stores')).toBe(true);
    expect(TestBed.inject(Router).serializeUrl(check('/inventory') as UrlTree)).toBe('/super-admin/stores');
  });

  it('allows store screens after a store has been selected', () => {
    TestBed.inject(AuthService).currentStore.set({ storeId: 'shop-1', storeName: 'One', storeCode: 'ONE', role: 'Owner', primaryColor: '#0284c7' });
    expect(check('/inventory')).toBe(true);
  });

  it('blocks platform management for ordinary store users', () => {
    const auth = TestBed.inject(AuthService);
    auth.currentUser.update(user => ({ ...user!, isSuperAdmin: false }));
    expect(TestBed.inject(Router).serializeUrl(check('/super-admin/stores') as UrlTree)).toBe('/dashboard');
  });
});
