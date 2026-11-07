import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { PurchaseService } from './purchase.service';
import { PurchaseOrder, PurchaseStatus } from './purchase.models';

describe('PurchaseService API status handling', () => {
  let service: PurchaseService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(PurchaseService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('does not leave a previous purchase actionable after navigation to a missing purchase', () => {
    service.getOrderById('po-1').subscribe();
    http.expectOne('/api/purchases/po-1').flush({ data: { id: 'po-1', status: 'Offered', items: [], payments: [] } });
    service.getOrderById('missing').subscribe({ error: () => {} });
    expect(service.selectedOrder()).toBeNull();
    http.expectOne('/api/purchases/missing').flush({ message: 'Not found' }, { status: 404, statusText: 'Not Found' });
    expect(service.selectedOrder()).toBeNull();
    expect(service.loading()).toBe(false);
  });

  it('keeps actionable numeric statuses throughout the purchase workflow', () => {
    service.getOrderById('po-1').subscribe();
    http.expectOne('/api/purchases/po-1').flush({ data: { id: 'po-1', status: 'Offered', items: [], payments: [] } });
    expect(service.selectedOrder()?.status).toBe(PurchaseStatus.Offered);

    service.acceptOffer('po-1').subscribe();
    http.expectOne('/api/purchases/po-1/accept').flush({ data: { id: 'po-1', status: 'Accepted', items: [], payments: [] } });
    expect(service.selectedOrder()?.status).toBe(PurchaseStatus.Accepted);

    service.recordPayment('po-1', { paymentMethod: 1, amount: 50000 }).subscribe();
    http.expectOne('/api/purchases/po-1/payments').flush({ data: { id: 'po-1', status: 'Paid', items: [], payments: [] } });
    expect(service.selectedOrder()?.status).toBe(PurchaseStatus.Paid);

    service.receiveToStock('po-1').subscribe();
    http.expectOne('/api/purchases/po-1/receive').flush({ data: { id: 'po-1', status: 'Received', items: [], payments: [] } });
    expect(service.selectedOrder()?.status).toBe(PurchaseStatus.Received);
  });

  it('normalizes list statuses and preserves numeric responses', () => {
    let orders: PurchaseOrder[] = [];
    service.loadPurchases().subscribe(result => orders = result.items);
    http.expectOne('/api/purchases?page=1&pageSize=20').flush({
      data: { items: [{ status: 'Offered', items: [], payments: [] }, { status: PurchaseStatus.Accepted, items: [], payments: [] }] }
    });
    expect(orders.map(order => order.status)).toEqual([PurchaseStatus.Offered, PurchaseStatus.Accepted]);
    expect(service.pagedResult().items).toEqual(orders);
  });
});
