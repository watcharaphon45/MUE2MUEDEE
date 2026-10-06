import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap, map } from 'rxjs';
import { ApiResponse } from '../../core/models/api-response.model';
import {
  InventoryItem,
  CreateInventoryItemRequest,
  UpdateInventoryItemRequest,
  AdjustCostRequest,
  UpdateStatusRequest,
  StockMovement,
  InventoryQueryFilter,
  PagedResult,
  CostBreakdown,
  InventoryCost,
  AddCostItemRequest,
  PriceHistory,
  UpdatePriceRequest,
  PricingRecommendation
} from './inventory.models';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/inventory';

  readonly pagedResult = signal<PagedResult<InventoryItem>>({
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 0
  });

  readonly selectedItem = signal<InventoryItem | null>(null);
  readonly movements = signal<StockMovement[]>([]);
  readonly costBreakdown = signal<CostBreakdown | null>(null);
  readonly pricingRecommendation = signal<PricingRecommendation | null>(null);
  readonly priceHistories = signal<PriceHistory[]>([]);
  readonly loading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);

  loadInventory(filter: Partial<InventoryQueryFilter> = {}): Observable<PagedResult<InventoryItem>> {
    this.loading.set(true);
    this.errorMessage.set(null);

    let params = new HttpParams()
      .set('page', (filter.page ?? 1).toString())
      .set('pageSize', (filter.pageSize ?? 20).toString());

    if (filter.search) params = params.set('search', filter.search);
    if (filter.brandId) params = params.set('brandId', filter.brandId);
    if (filter.categoryId) params = params.set('categoryId', filter.categoryId);
    if (filter.productId) params = params.set('productId', filter.productId);
    if (filter.status !== undefined && filter.status !== null) {
      params = params.set('status', filter.status.toString());
    }
    if (filter.minPrice !== undefined) params = params.set('minPrice', filter.minPrice.toString());
    if (filter.maxPrice !== undefined) params = params.set('maxPrice', filter.maxPrice.toString());

    return this.http.get<ApiResponse<PagedResult<InventoryItem>>>(this.baseUrl, { params }).pipe(
      map(res => res.data),
      tap({
        next: result => {
          this.pagedResult.set(result);
          this.loading.set(false);
        },
        error: err => {
          this.errorMessage.set(err.error?.message ?? 'เกิดข้อผิดพลาดในการโหลดข้อมูลสินค้า');
          this.loading.set(false);
        }
      })
    );
  }

  getItemById(id: string): Observable<InventoryItem> {
    this.loading.set(true);
    return this.http.get<ApiResponse<InventoryItem>>(`${this.baseUrl}/${id}`).pipe(
      map(res => res.data),
      tap({
        next: item => {
          this.selectedItem.set(item);
          this.loading.set(false);
        },
        error: err => {
          this.errorMessage.set(err.error?.message ?? 'ไม่พบข้อมูลสินค้า');
          this.loading.set(false);
        }
      })
    );
  }

  createItem(req: CreateInventoryItemRequest): Observable<InventoryItem> {
    return this.http.post<ApiResponse<InventoryItem>>(this.baseUrl, req).pipe(
      map(res => res.data),
      tap(created => {
        this.pagedResult.update(p => ({
          ...p,
          items: [created, ...p.items],
          totalCount: p.totalCount + 1
        }));
      })
    );
  }

  updateItem(id: string, req: UpdateInventoryItemRequest): Observable<InventoryItem> {
    return this.http.put<ApiResponse<InventoryItem>>(`${this.baseUrl}/${id}`, req).pipe(
      map(res => res.data),
      tap(updated => {
        this.selectedItem.set(updated);
        this.pagedResult.update(p => ({
          ...p,
          items: p.items.map(i => (i.id === id ? updated : i))
        }));
      })
    );
  }

  adjustCost(id: string, req: AdjustCostRequest): Observable<InventoryItem> {
    return this.http.post<ApiResponse<InventoryItem>>(`${this.baseUrl}/${id}/adjust-cost`, req).pipe(
      map(res => res.data),
      tap(updated => {
        this.selectedItem.set(updated);
        this.pagedResult.update(p => ({
          ...p,
          items: p.items.map(i => (i.id === id ? updated : i))
        }));
      })
    );
  }

  updateStatus(id: string, req: UpdateStatusRequest): Observable<InventoryItem> {
    return this.http.post<ApiResponse<InventoryItem>>(`${this.baseUrl}/${id}/status`, req).pipe(
      map(res => res.data),
      tap(updated => {
        this.selectedItem.set(updated);
        this.pagedResult.update(p => ({
          ...p,
          items: p.items.map(i => (i.id === id ? updated : i))
        }));
      })
    );
  }

  loadMovements(id: string): Observable<StockMovement[]> {
    return this.http.get<ApiResponse<StockMovement[]>>(`${this.baseUrl}/${id}/movements`).pipe(
      map(res => res.data),
      tap(movements => this.movements.set(movements))
    );
  }

  deleteItem(id: string): Observable<void> {
    return this.http.delete<ApiResponse<unknown>>(`${this.baseUrl}/${id}`).pipe(
      map(() => void 0),
      tap(() => {
        this.pagedResult.update(p => ({
          ...p,
          items: p.items.filter(i => i.id !== id),
          totalCount: p.totalCount - 1
        }));
      })
    );
  }

  // ==========================================
  // Phase 4: Cost & Pricing Engine
  // ==========================================

  getCostBreakdown(id: string): Observable<CostBreakdown> {
    return this.http.get<ApiResponse<CostBreakdown>>(`${this.baseUrl}/${id}/costs`).pipe(
      map(res => res.data),
      tap(data => this.costBreakdown.set(data))
    );
  }

  addCostItem(id: string, req: AddCostItemRequest): Observable<InventoryCost> {
    return this.http.post<ApiResponse<InventoryCost>>(`${this.baseUrl}/${id}/costs`, req).pipe(
      map(res => res.data),
      tap(() => {
        this.getCostBreakdown(id).subscribe();
        this.getItemById(id).subscribe();
      })
    );
  }

  deleteCostItem(id: string, costId: string): Observable<void> {
    return this.http.delete<ApiResponse<unknown>>(`${this.baseUrl}/${id}/costs/${costId}`).pipe(
      map(() => void 0),
      tap(() => {
        this.getCostBreakdown(id).subscribe();
        this.getItemById(id).subscribe();
      })
    );
  }

  getPricingRecommendation(id: string, customMargin?: number): Observable<PricingRecommendation> {
    let params = new HttpParams();
    if (customMargin !== undefined && customMargin !== null) {
      params = params.set('customTargetMargin', customMargin.toString());
    }
    return this.http.get<ApiResponse<PricingRecommendation>>(`${this.baseUrl}/${id}/pricing-recommendation`, { params }).pipe(
      map(res => res.data),
      tap(data => this.pricingRecommendation.set(data))
    );
  }

  updatePrice(id: string, req: UpdatePriceRequest): Observable<PriceHistory> {
    return this.http.put<ApiResponse<PriceHistory>>(`${this.baseUrl}/${id}/price`, req).pipe(
      map(res => res.data),
      tap(() => {
        this.getItemById(id).subscribe();
        this.getPriceHistories(id).subscribe();
        this.getPricingRecommendation(id).subscribe();
      })
    );
  }

  getPriceHistories(id: string): Observable<PriceHistory[]> {
    return this.http.get<ApiResponse<PriceHistory[]>>(`${this.baseUrl}/${id}/price-history`).pipe(
      map(res => res.data),
      tap(histories => this.priceHistories.set(histories))
    );
  }
}
