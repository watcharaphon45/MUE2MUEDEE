import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { InventoryListComponent } from './inventory-list.component';

describe('Inventory table', () => {
  async function createTable() {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])] });
    const fixture = TestBed.createComponent(InventoryListComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne('/api/master-data/brands').flush({ data: [] });
    http.expectOne('/api/master-data/categories').flush({ data: [] });
    http.expectOne('/api/sticker-templates').flush({ data: [] });
    http.expectOne(req => req.url === '/api/inventory').flush({ data: {
      items: [{ id: 'item-1', sku: 'CAM-001', productModelName: 'Sony Alpha 7 IV',
        serialNumber: 'SN-001', status: 3, statusName: 'พร้อมขาย', shutterCount: 0, actualCost: 50000, purchaseCost: 50000,
        sellingPrice: 60000, expectedProfit: 10000, expectedMarginPercent: 16.67 }],
      totalCount: 61, page: 1, pageSize: 15, totalPages: 5
    } });
    await fixture.whenStable();
    fixture.detectChanges();
    return { fixture, component: fixture.componentInstance, http };
  }

  it('shows a recorded shutter count of zero', async () => {
    const { fixture, http } = await createTable();
    expect(fixture.nativeElement.querySelector('.shutter-count')?.textContent).toContain('0 sc');
    http.verify();
  });

  it('clears stale selections when a new search is submitted and retains a zero price filter', async () => {
    const { component, http } = await createTable();
    component.toggleSelectItem('item-1');
    component.filterForm.patchValue({ search: 'lens', minPrice: 0, maxPrice: 0 });
    component.search();
    const request = http.expectOne(req => req.url === '/api/inventory');
    expect(component.selectedItemsCount()).toBe(0);
    expect(component.selectedInventoryItems()).toEqual([]);
    expect(request.request.params.get('search')).toBe('lens');
    expect(request.request.params.get('minPrice')).toBe('0');
    expect(request.request.params.get('maxPrice')).toBe('0');
    expect(request.request.params.get('page')).toBe('1');
    request.flush({ data: { items: [], totalCount: 0, page: 1, pageSize: 15, totalPages: 0 } });
    http.verify();
  });

  it('requests the selected page with the active filters and clears page selections', async () => {
    const { fixture, component, http } = await createTable();
    component.filterForm.patchValue({ search: 'Sony', maxPrice: 80000 });
    component.toggleSelectItem('item-1');
    (fixture.nativeElement.querySelector('app-table-pagination [aria-label="หน้าถัดไป"]') as HTMLButtonElement).click();
    const request = http.expectOne(req => req.url === '/api/inventory');
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('15');
    expect(request.request.params.get('search')).toBe('Sony');
    expect(request.request.params.get('maxPrice')).toBe('80000');
    expect(component.selectedItemsCount()).toBe(0);
    request.flush({ data: { items: [], totalCount: 61, page: 2, pageSize: 15, totalPages: 5 } });
    http.verify();
  });
});
