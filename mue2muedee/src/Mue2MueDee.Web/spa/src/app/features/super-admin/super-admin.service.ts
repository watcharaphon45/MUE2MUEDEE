import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, map } from 'rxjs';
import { CreateStoreWithOwnerPayload, SuperAdminStats, SuperAdminStoreItem } from './super-admin.models';
import { ApiResponse } from '../../core/models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class SuperAdminService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/super-admin';

  readonly stores = signal<SuperAdminStoreItem[]>([]);
  readonly stats = signal<SuperAdminStats | null>(null);
  readonly isLoading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);
  readonly statsErrorMessage = signal<string | null>(null);

  loadStores(): Observable<SuperAdminStoreItem[]> {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    return this.http.get<ApiResponse<SuperAdminStoreItem[]>>(`${this.apiUrl}/stores`).pipe(
      map(res => res.data),
      tap({
        next: (items) => {
          this.stores.set(items);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
          this.errorMessage.set(err?.error?.message ?? 'เกิดข้อผิดพลาดในการโหลดข้อมูลร้านค้า');
        }
      })
    );
  }

  loadStats(): Observable<SuperAdminStats> {
    this.statsErrorMessage.set(null);
    return this.http.get<ApiResponse<SuperAdminStats>>(`${this.apiUrl}/stats`).pipe(
      map(res => res.data),
      tap({
        next: stats => this.stats.set(stats),
        error: err => this.statsErrorMessage.set(err?.error?.message ?? 'เกิดข้อผิดพลาดในการโหลดสรุปร้านค้า')
      })
    );
  }

  createStoreWithOwner(payload: CreateStoreWithOwnerPayload): Observable<SuperAdminStoreItem> {
    return this.http.post<ApiResponse<SuperAdminStoreItem>>(`${this.apiUrl}/stores`, payload).pipe(
      map(res => res.data),
      tap(created => {
        this.stores.update(current => [created, ...current]);
        this.loadStats().subscribe({ error: () => {} });
      })
    );
  }

  toggleStoreStatus(storeId: string): Observable<boolean> {
    return this.http.put<ApiResponse<boolean>>(`${this.apiUrl}/stores/${storeId}/toggle-status`, {}).pipe(
      map(res => res.data),
      tap(newStatus => {
        this.stores.update(current =>
          current.map(s => s.id === storeId ? { ...s, isActive: newStatus } : s)
        );
        this.loadStats().subscribe({ error: () => {} });
      })
    );
  }
}
