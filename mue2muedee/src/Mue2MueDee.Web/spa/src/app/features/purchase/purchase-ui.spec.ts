import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { PurchaseListComponent } from './purchase-list/purchase-list.component';
import { PurchaseDetailComponent } from './purchase-detail/purchase-detail.component';
import { PurchaseWizardComponent } from './purchase-wizard/purchase-wizard.component';
import { InspectionItemResult, PurchaseOrder, PurchaseStatus } from './purchase.models';

describe('Purchase screens', () => {
  const order = (status = PurchaseStatus.Offered): PurchaseOrder => ({
    id: 'po-1', poNumber: 'PO-2611-0001', customerId: 'customer-1', customerName: 'ผู้ขายทดสอบ',
    customerPhone: '0812345678', status, statusName: PurchaseStatus[status], needsApproval: false,
    totalEstimatedValue: 120, totalMaxBuyPrice: 110, totalOfferedPrice: 100.5, totalAgreedPrice: 100.5,
    purchaseDate: '2026-11-07T02:00:00Z', createdAt: '2026-11-07T02:00:00Z', payments: [],
    items: [{ id: 'item-1', purchaseOrderId: 'po-1', productId: 'product-1', productModelName: 'Alpha 7 IV',
      productCode: 'A7IV', brandName: 'Sony', categoryName: 'กล้อง', categoryType: 1, serialNumber: 'SN-001',
      conditionGradeId: 'grade-1', conditionGradeCode: 'A', conditionGradeName: 'ดี', shutterCount: 0,
      expectedSalePrice: 120, estimatedRepairCost: 0, estimatedCleaningCost: 0, missingAccessoriesDeduction: 0,
      maxBuyPrice: 110, offeredPrice: 100.5, agreedPrice: 100.5, inspectionResults: [] }]
  });
  const paged = (page = 1, pageSize = 15) => ({ items: [order()], totalCount: 61, page, pageSize, totalPages: Math.ceil(61 / pageSize) });

  beforeEach(() => TestBed.configureTestingModule({ providers: [
    provideHttpClient(), provideHttpClientTesting(), provideRouter([]),
    { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ id: 'po-1' }) } } }
  ] }));

  it('retains filters and Bangkok date boundaries when navigating the purchase list', async () => {
    const fixture = TestBed.createComponent(PurchaseListComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne(req => req.url === '/api/purchases').flush({ data: paged() });
    await fixture.whenStable();
    fixture.componentInstance.filterForm.patchValue({ search: 'SN-001', status: '1', fromDate: '2026-11-07', toDate: '2026-11-07' });
    fixture.detectChanges();
    (fixture.nativeElement.querySelector('[aria-label="หน้าถัดไป"]') as HTMLButtonElement).click();
    const request = http.expectOne(req => req.url === '/api/purchases');
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('15');
    expect(request.request.params.get('search')).toBe('SN-001');
    expect(request.request.params.get('status')).toBe('1');
    expect(request.request.params.get('fromDate')).toBe('2026-11-07T00:00:00.000+07:00');
    expect(request.request.params.get('toDate')).toBe('2026-11-07T23:59:59.999+07:00');
    request.flush({ data: paged(2) });
    http.verify();
  });

  it('distinguishes failed loading from an empty purchase list', () => {
    const fixture = TestBed.createComponent(PurchaseListComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne(req => req.url === '/api/purchases').flush({ message: 'โหลดใบรับซื้อไม่สำเร็จ' }, { status: 503, statusText: 'Unavailable' });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain('โหลดใบรับซื้อไม่สำเร็จ');
    expect(fixture.nativeElement.textContent).not.toContain('ไม่พบรายการใบรับซื้อ');
    http.verify();
  });

  async function detail(po: PurchaseOrder) {
    const fixture = TestBed.createComponent(PurchaseDetailComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne('/api/purchases/po-1').flush({ data: po });
    await fixture.whenStable();
    fixture.detectChanges();
    return { fixture, component: fixture.componentInstance, http };
  }

  it('accepts an offer then receives stock through the existing PO, sending each action once', async () => {
    const { fixture, component, http } = await detail(order());
    expect(fixture.nativeElement.textContent).toContain('ชัตเตอร์: 0 sc');
    component.acceptOffer();
    component.acceptOffer();
    http.expectOne('/api/purchases/po-1/accept').flush({ data: order(PurchaseStatus.Accepted) });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('นำเข้าคลังสินค้า');
    component.receiveToStock();
    component.receiveToStock();
    const received = order(PurchaseStatus.Received);
    received.items[0].inventoryItemId = 'stock-1';
    received.items[0].generatedSku = 'CAM-2611-0001';
    http.expectOne('/api/purchases/po-1/receive').flush({ data: received });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('CAM-2611-0001');
    expect(fixture.nativeElement.querySelector('.btn-receive')).toBeNull();
    http.verify();
  });

  it('allows a final payment below one baht and prevents a duplicate submission', async () => {
    const po = order(PurchaseStatus.Accepted);
    po.payments = [{ id: 'payment-1', amount: 100, paymentMethod: 2, paymentMethodName: 'โอน', paidAt: po.createdAt }];
    const { component, http } = await detail(po);
    component.openPayment();
    expect(component.paymentForm.controls.amount.value).toBe(0.5);
    expect(component.paymentForm.valid).toBe(true);
    component.submitPayment();
    component.submitPayment();
    component.closePayment();
    expect(component.showPaymentModal()).toBe(true);
    const request = http.expectOne('/api/purchases/po-1/payments');
    expect(request.request.body.amount).toBe(0.5);
    request.flush({ data: { ...po, status: PurchaseStatus.Paid } });
    expect(component.showPaymentModal()).toBe(false);
    http.verify();
  });

  async function wizard() {
    const fixture = TestBed.createComponent(PurchaseWizardComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    for (const url of ['/api/customers', '/api/master-data/products', '/api/master-data/condition-grades', '/api/master-data/categories']) {
      http.expectOne(req => req.url === url).flush({ data: [] });
    }
    await fixture.whenStable();
    const component = fixture.componentInstance;
    component.step1Form.patchValue({ customerId: 'customer-1' });
    component.step2Form.patchValue({ productId: 'product-1', serialNumber: 'SN-001', conditionGradeId: 'grade-1', shutterCount: 0 });
    component.step3Form.patchValue({ expectedSalePrice: 120, offeredPrice: 100.5 });
    component.inspectionAnswers.set(new Map([['check-1', { result: InspectionItemResult.Pass, notes: '' }]]));
    return { fixture, component, http };
  }

  it('waits for inspection to save before offering and retries failed inspection on the same PO', async () => {
    const { component, http } = await wizard();
    component.createAndSubmitPO();
    component.createAndSubmitPO();
    const create = http.expectOne('/api/purchases');
    expect(create.request.body.initialItems[0].shutterCount).toBe(0);
    create.flush({ data: order(PurchaseStatus.Appraising) });
    http.expectNone('/api/purchases/po-1/offer');
    http.expectOne('/api/purchases/po-1/items/item-1/inspection').flush({ message: 'บันทึกผลตรวจไม่สำเร็จ' }, { status: 500, statusText: 'Error' });
    expect(component.errorMessage()).toBe('บันทึกผลตรวจไม่สำเร็จ');
    expect(component.hasPendingSubmission()).toBe(true);
    expect(component.isSubmitting()).toBe(false);
    component.completeSubmission();
    http.expectNone('/api/purchases');
    http.expectOne('/api/purchases/po-1/items/item-1/inspection').flush({ data: order(PurchaseStatus.Appraising) });
    http.expectOne('/api/purchases/po-1/offer').flush({ data: order() });
    expect(component.hasPendingSubmission()).toBe(false);
    expect(component.createdPO()?.status).toBe(PurchaseStatus.Offered);
    http.verify();
  });

  it('reports a failed offer and keeps the existing PO available for retry', async () => {
    const { fixture, component, http } = await wizard();
    component.createAndSubmitPO();
    http.expectOne('/api/purchases').flush({ data: order(PurchaseStatus.Appraising) });
    http.expectOne('/api/purchases/po-1/items/item-1/inspection').flush({ data: order(PurchaseStatus.Appraising) });
    http.expectOne('/api/purchases/po-1/offer').flush({ message: 'เสนอราคาไม่สำเร็จ' }, { status: 500, statusText: 'Error' });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('เสนอราคาไม่สำเร็จ');
    expect(fixture.nativeElement.textContent).toContain('ลองบันทึกผลตรวจและเสนอราคาอีกครั้ง');
    component.goToStep(1);
    component.createAndSubmitPO();
    expect(component.currentStep()).toBe(4);
    http.expectNone('/api/purchases');
    http.verify();
  });
});
