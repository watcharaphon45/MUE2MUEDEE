import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { InventoryService } from './inventory.service';
import { InventoryStatus } from './inventory.models';

describe('InventoryService status handling', () => {
  it('converts API status names before populating the status form and after updates', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const service = TestBed.inject(InventoryService);
    const http = TestBed.inject(HttpTestingController);
    service.getItemById('item-1').subscribe();
    http.expectOne('/api/inventory/item-1').flush({ data: { status: 'Available' } });
    expect(service.selectedItem()?.status).toBe(InventoryStatus.Available);
    service.updateStatus('item-1', { newStatus: InventoryStatus.Repair }).subscribe();
    http.expectOne('/api/inventory/item-1/status').flush({ data: { status: 'Repair' } });
    expect(service.selectedItem()?.status).toBe(InventoryStatus.Repair);
    http.verify();
  });
});
