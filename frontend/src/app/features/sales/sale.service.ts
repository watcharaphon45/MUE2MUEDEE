import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap, map } from 'rxjs';
import { ApiResponse } from '../../core/models/api-response.model';
import {
  SaleOrder,
  CreateSaleOrderRequest,
  SaleQueryFilter,
  PagedResult,
  SerialTrace
} from './sale.models';
import { InventoryItem } from '../inventory/inventory.models';

@Injectable({
  providedIn: 'root'
})
export class SaleService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/sales';

  readonly pagedResult = signal<PagedResult<SaleOrder>>({
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 0
  });

  readonly selectedSale = signal<SaleOrder | null>(null);
  readonly serialTraceResult = signal<SerialTrace | null>(null);
  readonly loading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);

  loadSales(filter: Partial<SaleQueryFilter> = {}): Observable<PagedResult<SaleOrder>> {
    this.loading.set(true);
    this.errorMessage.set(null);

    let params = new HttpParams()
      .set('page', (filter.page ?? 1).toString())
      .set('pageSize', (filter.pageSize ?? 20).toString());

    if (filter.search) params = params.set('search', filter.search);
    if (filter.status !== undefined && filter.status !== null) {
      params = params.set('status', filter.status.toString());
    }
    if (filter.startDate) params = params.set('startDate', filter.startDate);
    if (filter.endDate) params = params.set('endDate', filter.endDate);

    return this.http.get<ApiResponse<PagedResult<SaleOrder>>>(this.baseUrl, { params }).pipe(
      map(res => res.data),
      tap({
        next: result => {
          this.pagedResult.set(result);
          this.loading.set(false);
        },
        error: err => {
          this.errorMessage.set(err.error?.message ?? 'เกิดข้อผิดพลาดในการโหลดรายการขาย');
          this.loading.set(false);
        }
      })
    );
  }

  getSaleById(id: string): Observable<SaleOrder> {
    this.loading.set(true);
    return this.http.get<ApiResponse<SaleOrder>>(`${this.baseUrl}/${id}`).pipe(
      map(res => res.data),
      tap({
        next: sale => {
          this.selectedSale.set(sale);
          this.loading.set(false);
        },
        error: err => {
          this.errorMessage.set(err.error?.message ?? 'ไม่พบข้อมูลรายการขาย');
          this.loading.set(false);
        }
      })
    );
  }

  createSale(request: CreateSaleOrderRequest): Observable<SaleOrder> {
    return this.http.post<ApiResponse<SaleOrder>>(this.baseUrl, request).pipe(
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

  cancelSale(id: string, reason: string): Observable<SaleOrder> {
    return this.http.post<ApiResponse<SaleOrder>>(`${this.baseUrl}/${id}/cancel`, { reason }).pipe(
      map(res => res.data),
      tap(updated => {
        this.selectedSale.set(updated);
        this.pagedResult.update(p => ({
          ...p,
          items: p.items.map(s => (s.id === id ? updated : s))
        }));
      })
    );
  }

  getSerialTrace(serialNumber: string): Observable<SerialTrace> {
    this.loading.set(true);
    this.errorMessage.set(null);
    return this.http.get<ApiResponse<SerialTrace>>(`${this.baseUrl}/trace/${encodeURIComponent(serialNumber)}`).pipe(
      map(res => res.data),
      tap({
        next: trace => {
          this.serialTraceResult.set(trace);
          this.loading.set(false);
        },
        error: err => {
          this.errorMessage.set(err.error?.message ?? `ไม่พบประวัติสินค้าสำหรับ Serial: ${serialNumber}`);
          this.serialTraceResult.set(null);
          this.loading.set(false);
        }
      })
    );
  }

  searchAvailableItems(query: string): Observable<InventoryItem[]> {
    const params = new HttpParams()
      .set('search', query)
      .set('status', '3') // Available = 3
      .set('pageSize', '10');

    return this.http.get<ApiResponse<PagedResult<InventoryItem>>>('/api/inventory', { params }).pipe(
      map(res => res.data.items)
    );
  }

  reserveItem(inventoryItemId: string, customerName?: string, notes?: string): Observable<void> {
    return this.http.post<ApiResponse<unknown>>(`${this.baseUrl}/reserve/${inventoryItemId}`, {
      customerName,
      notes
    }).pipe(map(() => void 0));
  }

  releaseReservation(inventoryItemId: string, reason?: string): Observable<void> {
    return this.http.post<ApiResponse<unknown>>(`${this.baseUrl}/release-reservation/${inventoryItemId}`, {
      reason
    }).pipe(map(() => void 0));
  }
}
