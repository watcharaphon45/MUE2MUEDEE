import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap, map, catchError, of } from 'rxjs';
import { ApiResponse } from '../../core/models/api-response.model';
import { SalesReport, SalesReportQuery, StockAgingReport } from './reports.models';

@Injectable({
  providedIn: 'root'
})
export class ReportsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/analytics';

  readonly salesReport = signal<SalesReport | null>(null);
  readonly stockAgingReport = signal<StockAgingReport | null>(null);
  readonly loading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);

  loadSalesReport(query: SalesReportQuery = {}): Observable<SalesReport | null> {
    this.loading.set(true);
    this.errorMessage.set(null);

    let params = new HttpParams();
    if (query.startDate) params = params.set('startDate', query.startDate);
    if (query.endDate) params = params.set('endDate', query.endDate);
    if (query.brandId) params = params.set('brandId', query.brandId);
    if (query.categoryId) params = params.set('categoryId', query.categoryId);
    if (query.paymentMethod !== undefined && query.paymentMethod !== null) {
      params = params.set('paymentMethod', query.paymentMethod.toString());
    }

    return this.http.get<ApiResponse<SalesReport>>(`${this.baseUrl}/sales-report`, { params }).pipe(
      map(res => res.data),
      tap({
        next: (data) => {
          this.salesReport.set(data);
          this.loading.set(false);
        },
        error: (err) => {
          this.errorMessage.set(err?.error?.message || 'ไม่สามารถโหลดรายงานยอดขายได้');
          this.loading.set(false);
        }
      }),
      catchError(() => of(null))
    );
  }

  loadStockAgingReport(): Observable<StockAgingReport | null> {
    this.loading.set(true);
    this.errorMessage.set(null);

    return this.http.get<ApiResponse<StockAgingReport>>(`${this.baseUrl}/stock-aging`).pipe(
      map(res => res.data),
      tap({
        next: (data) => {
          this.stockAgingReport.set(data);
          this.loading.set(false);
        },
        error: (err) => {
          this.errorMessage.set(err?.error?.message || 'ไม่สามารถโหลดรายงานอายุสต็อกได้');
          this.loading.set(false);
        }
      }),
      catchError(() => of(null))
    );
  }

  downloadSalesReportCsv(query: SalesReportQuery = {}): Observable<Blob> {
    let params = new HttpParams();
    if (query.startDate) params = params.set('startDate', query.startDate);
    if (query.endDate) params = params.set('endDate', query.endDate);
    if (query.brandId) params = params.set('brandId', query.brandId);
    if (query.categoryId) params = params.set('categoryId', query.categoryId);
    if (query.paymentMethod !== undefined && query.paymentMethod !== null) {
      params = params.set('paymentMethod', query.paymentMethod.toString());
    }

    return this.http.get(`${this.baseUrl}/sales-report/export-csv`, {
      params,
      responseType: 'blob'
    }).pipe(
      tap((blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `sales_report_${new Date().toISOString().slice(0, 10)}.csv`;
        a.click();
        window.URL.revokeObjectURL(url);
      })
    );
  }

  downloadStockAgingCsv(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/stock-aging/export-csv`, {
      responseType: 'blob'
    }).pipe(
      tap((blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `stock_aging_${new Date().toISOString().slice(0, 10)}.csv`;
        a.click();
        window.URL.revokeObjectURL(url);
      })
    );
  }
}
