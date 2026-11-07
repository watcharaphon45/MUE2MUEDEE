import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { SuperAdminStoresComponent } from './super-admin-stores.component';
import { SuperAdminStoreItem } from '../super-admin.models';

describe('Store and owner management', () => {
  const stores: SuperAdminStoreItem[] = Array.from({ length: 32 }, (_, index) => ({
    id: `store-${index}`, name: `ร้านกล้อง ${index}`, code: `SHOP-${index}`, primaryColor: '#0284c7',
    isActive: true, createdAt: '2026-11-07T02:00:00Z', totalUsersCount: 2,
    ownerFullName: `เจ้าของร้าน ${index}`, ownerUsername: `owner${index}`,
    ownerEmail: `owner${index}@example.test`, ownerPhoneNumber: `08000000${String(index).padStart(2, '0')}`
  }));
  const stats = { totalStores: 32, activeStores: 32, totalOwners: 32, totalUsers: 64 };

  async function createPage(failLoad = false) {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const fixture = TestBed.createComponent(SuperAdminStoresComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    const request = http.expectOne('/api/super-admin/stores');
    if (failLoad) request.flush({ message: 'ไม่สามารถเชื่อมต่อได้' }, { status: 503, statusText: 'Unavailable' });
    else request.flush({ data: stores });
    http.expectOne('/api/super-admin/stats').flush({ data: stats });
    await fixture.whenStable();
    fixture.detectChanges();
    return { fixture, component: fixture.componentInstance, http };
  }

  it('filters owner contact details from page two and resets the visible page', async () => {
    const { fixture, component, http } = await createPage();
    (fixture.nativeElement.querySelector('[aria-label="หน้าถัดไป"]') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('tbody')?.textContent).toContain('SHOP-15');
    component.searchControl.setValue(stores[31].ownerPhoneNumber!);
    fixture.detectChanges();
    const rows = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(rows.length).toBe(1);
    expect(rows[0].textContent).toContain('SHOP-31');
    expect(fixture.nativeElement.querySelector('.page-range')?.textContent).toContain('แสดง 1–1 จาก 1');
    component.searchControl.setValue('missing');
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('ลองเปลี่ยนคำค้นหา');
    http.verify();
  });

  it('shows a load failure separately from an empty result and supports retry', async () => {
    const { fixture, component, http } = await createPage(true);
    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain('ไม่สามารถเชื่อมต่อได้');
    expect(fixture.nativeElement.textContent).not.toContain('ไม่พบข้อมูลร้านค้า');
    component.loadData();
    http.expectOne('/api/super-admin/stores').flush({ data: stores });
    http.expectOne('/api/super-admin/stats').flush({ data: stats });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('tbody tr').length).toBe(15);
    http.verify();
  });

  it('sends one status request per store, preserves the page and reports a failed statistics refresh', async () => {
    const { fixture, component, http } = await createPage();
    const confirmation = vi.spyOn(window, 'confirm').mockReturnValue(true);
    (fixture.nativeElement.querySelector('[aria-label="หน้าถัดไป"]') as HTMLButtonElement).click();
    fixture.detectChanges();
    component.toggleStatus(stores[15]);
    component.toggleStatus(stores[15]);
    fixture.detectChanges();
    expect((fixture.nativeElement.querySelector('tbody tr button') as HTMLButtonElement).disabled).toBe(true);
    const request = http.expectOne('/api/super-admin/stores/store-15/toggle-status');
    expect(request.request.method).toBe('PUT');
    request.flush({ data: false });
    http.expectOne('/api/super-admin/stats').flush({ message: 'โหลดสรุปไม่สำเร็จ' }, { status: 503, statusText: 'Unavailable' });
    fixture.detectChanges();
    expect(component.pendingStoreIds().size).toBe(0);
    expect(fixture.nativeElement.querySelector('tbody tr')?.textContent).toContain('SHOP-15');
    expect(fixture.nativeElement.querySelector('tbody tr')?.textContent).toContain('ระงับบริการ');
    expect(fixture.nativeElement.textContent).toContain('โหลดสรุปไม่สำเร็จ');
    confirmation.mockRestore();
    http.verify();
  });

  it('releases a failed status request and keeps the existing status', async () => {
    const { fixture, component, http } = await createPage();
    const confirmation = vi.spyOn(window, 'confirm').mockReturnValue(true);
    component.toggleStatus(stores[0]);
    http.expectOne('/api/super-admin/stores/store-0/toggle-status').flush({ message: 'ไม่มีสิทธิ์' }, { status: 403, statusText: 'Forbidden' });
    fixture.detectChanges();
    expect(component.pendingStoreIds().size).toBe(0);
    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain('ไม่มีสิทธิ์');
    expect(fixture.nativeElement.querySelector('tbody tr')?.textContent).toContain('เปิดใช้งาน');
    confirmation.mockRestore();
    http.verify();
  });

  it('requires an explicit password, prevents duplicate creation and clears the password after success', async () => {
    const { fixture, component, http } = await createPage();
    component.openCreateModal();
    expect(component.storeForm.controls.ownerPassword.value).toBe('');
    component.storeForm.patchValue({ storeName: 'ร้านใหม่', ownerFullName: 'เจ้าของใหม่', ownerUsername: 'newowner', ownerEmail: 'new@example.test' });
    component.onSubmit();
    http.expectNone(req => req.method === 'POST');
    component.storeForm.controls.ownerPassword.setValue('test-password');
    component.onSubmit();
    component.onSubmit();
    component.closeModal();
    expect(component.isModalOpen()).toBe(true);
    const request = http.expectOne('/api/super-admin/stores');
    expect(request.request.method).toBe('POST');
    expect(request.request.body.ownerPassword).toBe('test-password');
    request.flush({ data: { ...stores[0], id: 'new-store', name: 'ร้านใหม่' } });
    http.expectOne('/api/super-admin/stats').flush({ data: stats });
    fixture.detectChanges();
    expect(component.isModalOpen()).toBe(false);
    expect(component.storeForm.controls.ownerPassword.value).toBe('');
    expect(fixture.nativeElement.querySelector('tbody tr')?.textContent).toContain('ร้านใหม่');
    http.verify();
  });
});
