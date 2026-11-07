import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { SaleService } from './sale.service';
import { SaleStatus } from './sale.models';
import { InventoryStatus } from '../inventory/inventory.models';

describe('SaleService API enums', () => {
  let service: SaleService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(SaleService);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('loads a completed sale with the numeric status needed by the cancel button', () => {
    service.getSaleById('sale-1').subscribe();
    http.expectOne('/api/sales/sale-1').flush({ data: { status: 'Completed', payments: [{ paymentMethod: 'Cash' }] } });
    expect(service.selectedSale()?.status).toBe(SaleStatus.Completed);
    expect(service.selectedSale()?.payments[0].paymentMethod).toBe(1);

    service.cancelSale('sale-1', 'Customer cancelled').subscribe();
    http.expectOne('/api/sales/sale-1/cancel').flush({ data: { status: 'Cancelled', payments: [] } });
    expect(service.selectedSale()?.status).toBe(SaleStatus.Cancelled);
  });

  it('normalizes POS inventory search results', () => {
    let status: InventoryStatus | undefined;
    service.searchAvailableItems('Sony').subscribe(items => status = items[0].status);
    http.expectOne('/api/inventory?search=Sony&status=3&pageSize=10')
      .flush({ data: { items: [{ status: 'Available' }] } });
    expect(status).toBe(InventoryStatus.Available);
  });
});
