import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, map, catchError, of } from 'rxjs';
import { ApiResponse } from '../../core/models/api-response.model';
import { DashboardMetrics, StockAgingSummary } from './dashboard.models';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/analytics';

  readonly metrics = signal<DashboardMetrics | null>(null);
  readonly loading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);

  loadMetrics(): Observable<DashboardMetrics | null> {
    this.loading.set(true);
    this.errorMessage.set(null);

    return this.http.get<ApiResponse<DashboardMetrics>>(`${this.baseUrl}/dashboard`).pipe(
      map(res => res.data),
      tap({
        next: (data) => {
          this.metrics.set(data);
          this.loading.set(false);
        },
        error: (err) => {
          this.errorMessage.set(err?.error?.message || 'ไม่สามารถโหลดข้อมูลสถิติภาพรวมได้');
          this.loading.set(false);
        }
      }),
      catchError(() => of(null))
    );
  }
}
