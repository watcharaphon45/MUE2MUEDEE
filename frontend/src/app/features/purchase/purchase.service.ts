import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap, map } from 'rxjs';
import { ApiResponse } from '../../core/models/api-response.model';
import { PagedResult } from '../inventory/inventory.models';
import {
  PurchaseOrder,
  InspectionTemplate,
  CategoryType,
  CreatePurchaseOrderRequest,
  CreatePurchaseItemRequest,
  SaveInspectionItemRequest,
  MakeOfferRequest,
  RecordPurchasePaymentRequest,
  PurchaseQueryFilter
} from './purchase.models';

@Injectable({
  providedIn: 'root'
})
export class PurchaseService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/purchases';

  readonly pagedResult = signal<PagedResult<PurchaseOrder>>({
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 0
  });

  readonly selectedOrder = signal<PurchaseOrder | null>(null);
  readonly currentTemplate = signal<InspectionTemplate | null>(null);
  readonly loading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);

  loadPurchases(filter: Partial<PurchaseQueryFilter> = {}): Observable<PagedResult<PurchaseOrder>> {
    this.loading.set(true);
    this.errorMessage.set(null);

    let params = new HttpParams()
      .set('page', (filter.page ?? 1).toString())
      .set('pageSize', (filter.pageSize ?? 20).toString());

    if (filter.search) params = params.set('search', filter.search);
    if (filter.status !== undefined && filter.status !== null) {
      params = params.set('status', filter.status.toString());
    }
    if (filter.customerId) params = params.set('customerId', filter.customerId);
    if (filter.fromDate) params = params.set('fromDate', filter.fromDate);
    if (filter.toDate) params = params.set('toDate', filter.toDate);

    return this.http.get<ApiResponse<PagedResult<PurchaseOrder>>>(this.baseUrl, { params }).pipe(
      map(res => res.data),
      tap({
        next: result => {
          this.pagedResult.set(result);
          this.loading.set(false);
        },
        error: err => {
          this.errorMessage.set(err.error?.message ?? 'เกิดข้อผิดพลาดในการโหลดรายการใบรับซื้อ');
          this.loading.set(false);
        }
      })
    );
  }

  getOrderById(id: string): Observable<PurchaseOrder> {
    this.loading.set(true);
    return this.http.get<ApiResponse<PurchaseOrder>>(`${this.baseUrl}/${id}`).pipe(
      map(res => res.data),
      tap({
        next: order => {
          this.selectedOrder.set(order);
          this.loading.set(false);
        },
        error: err => {
          this.errorMessage.set(err.error?.message ?? 'ไม่พบใบรับซื้อ');
          this.loading.set(false);
        }
      })
    );
  }

  createOrder(req: CreatePurchaseOrderRequest): Observable<PurchaseOrder> {
    return this.http.post<ApiResponse<PurchaseOrder>>(this.baseUrl, req).pipe(
      map(res => res.data),
      tap(created => {
        this.selectedOrder.set(created);
        this.pagedResult.update(p => ({
          ...p,
          items: [created, ...p.items],
          totalCount: p.totalCount + 1
        }));
      })
    );
  }

  addItem(poId: string, req: CreatePurchaseItemRequest): Observable<PurchaseOrder> {
    return this.http.post<ApiResponse<PurchaseOrder>>(`${this.baseUrl}/${poId}/items`, req).pipe(
      map(res => res.data),
      tap(updated => this.selectedOrder.set(updated))
    );
  }

  removeItem(poId: string, itemId: string): Observable<PurchaseOrder> {
    return this.http.delete<ApiResponse<PurchaseOrder>>(`${this.baseUrl}/${poId}/items/${itemId}`).pipe(
      map(res => res.data),
      tap(updated => this.selectedOrder.set(updated))
    );
  }

  loadInspectionTemplate(categoryType: CategoryType): Observable<InspectionTemplate> {
    return this.http.get<ApiResponse<InspectionTemplate>>(`${this.baseUrl}/inspection-template/${categoryType}`).pipe(
      map(res => res.data),
      tap(tpl => this.currentTemplate.set(tpl))
    );
  }

  saveInspection(poId: string, itemId: string, results: SaveInspectionItemRequest[]): Observable<PurchaseOrder> {
    return this.http.post<ApiResponse<PurchaseOrder>>(`${this.baseUrl}/${poId}/items/${itemId}/inspection`, { results }).pipe(
      map(res => res.data),
      tap(updated => this.selectedOrder.set(updated))
    );
  }

  makeOffer(poId: string, req: MakeOfferRequest): Observable<PurchaseOrder> {
    return this.http.post<ApiResponse<PurchaseOrder>>(`${this.baseUrl}/${poId}/offer`, req).pipe(
      map(res => res.data),
      tap(updated => this.selectedOrder.set(updated))
    );
  }

  approvePurchase(poId: string): Observable<PurchaseOrder> {
    return this.http.post<ApiResponse<PurchaseOrder>>(`${this.baseUrl}/${poId}/approve`, {}).pipe(
      map(res => res.data),
      tap(updated => this.selectedOrder.set(updated))
    );
  }

  acceptOffer(poId: string): Observable<PurchaseOrder> {
    return this.http.post<ApiResponse<PurchaseOrder>>(`${this.baseUrl}/${poId}/accept`, {}).pipe(
      map(res => res.data),
      tap(updated => this.selectedOrder.set(updated))
    );
  }

  rejectOffer(poId: string): Observable<PurchaseOrder> {
    return this.http.post<ApiResponse<PurchaseOrder>>(`${this.baseUrl}/${poId}/reject`, {}).pipe(
      map(res => res.data),
      tap(updated => this.selectedOrder.set(updated))
    );
  }

  recordPayment(poId: string, req: RecordPurchasePaymentRequest): Observable<PurchaseOrder> {
    return this.http.post<ApiResponse<PurchaseOrder>>(`${this.baseUrl}/${poId}/payments`, req).pipe(
      map(res => res.data),
      tap(updated => this.selectedOrder.set(updated))
    );
  }

  receiveToStock(poId: string): Observable<PurchaseOrder> {
    return this.http.post<ApiResponse<PurchaseOrder>>(`${this.baseUrl}/${poId}/receive`, {}).pipe(
      map(res => res.data),
      tap(updated => this.selectedOrder.set(updated))
    );
  }

  deleteOrder(id: string): Observable<void> {
    return this.http.delete<ApiResponse<unknown>>(`${this.baseUrl}/${id}`).pipe(
      map(() => void 0),
      tap(() => {
        this.pagedResult.update(p => ({
          ...p,
          items: p.items.filter(po => po.id !== id),
          totalCount: p.totalCount - 1
        }));
      })
    );
  }

  // Frontend helper calculation formula matching backend specification
  static calculateMaxBuyPrice(
    expectedSale: number,
    repair: number = 0,
    cleaning: number = 0,
    missingAccessories: number = 0,
    targetMargin: number = 0.15
  ): number {
    const base = expectedSale * (1 - targetMargin) - repair - cleaning - missingAccessories;
    return Math.max(0, Math.floor(base / 100) * 100);
  }
}
