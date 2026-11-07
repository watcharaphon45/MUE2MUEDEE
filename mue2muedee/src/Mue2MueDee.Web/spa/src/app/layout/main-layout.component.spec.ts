import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { MainLayoutComponent } from './main-layout.component';

describe('Super Admin store switcher', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({ providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()] });
    const auth = TestBed.inject(AuthService);
    auth.currentUser.set({ id: 'admin', username: 'admin', fullName: 'Admin', email: 'admin@example.test', isSuperAdmin: true });
    auth.availableStores.set([{ storeId: 'shop-1', storeName: 'One', storeCode: 'ONE', role: 'Owner', primaryColor: '#0284c7' }]);
  });
  afterEach(() => localStorage.clear());

  it('offers the only store while hiding store menus in platform mode', () => {
    const fixture = TestBed.createComponent(MainLayoutComponent);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('header app-select')).not.toBeNull();
    expect(fixture.componentInstance.menuGroups().flatMap(group => group.items).map(item => item.route)).toEqual(['/super-admin/stores']);
  });

  it('prevents duplicate switching and keeps the existing context after a rejected switch', () => {
    const fixture = TestBed.createComponent(MainLayoutComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.componentInstance.onStoreSelect('shop-1');
    fixture.componentInstance.onStoreSelect('shop-1');
    http.expectOne('/api/auth/switch-store').flush({ message: 'ร้านค้านี้ถูกระงับแล้ว' }, { status: 403, statusText: 'Forbidden' });
    fixture.detectChanges();
    expect(fixture.componentInstance.isSwitchingStore()).toBe(false);
    expect(TestBed.inject(AuthService).currentStore()).toBeNull();
    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain('ร้านค้านี้ถูกระงับแล้ว');
    http.verify();
  });
});
