import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap, map } from 'rxjs';
import { PublicCatalogItem, PublicCatalogResponse, PublicStoreInfo, PublicCatalogFilterMeta } from './catalog.models';

@Injectable({
  providedIn: 'root'
})
export class CatalogService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/catalog';

  readonly store = signal<PublicStoreInfo | null>(null);
  readonly items = signal<PublicCatalogItem[]>([]);
  readonly filterMeta = signal<PublicCatalogFilterMeta | null>(null);
  readonly totalCount = signal<number>(0);
  readonly isLoading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);

  loadStoreCatalog(
    storeCode: string,
    filters: {
      search?: string;
      brandId?: string;
      categoryId?: string;
      conditionGradeId?: string;
      sortBy?: string;
      page?: number;
      pageSize?: number;
    } = {}
  ): Observable<PublicCatalogResponse> {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    let params = new HttpParams();
    if (filters.search) params = params.set('search', filters.search);
    if (filters.brandId) params = params.set('brandId', filters.brandId);
    if (filters.categoryId) params = params.set('categoryId', filters.categoryId);
    if (filters.conditionGradeId) params = params.set('conditionGradeId', filters.conditionGradeId);
    if (filters.sortBy) params = params.set('sortBy', filters.sortBy);
    if (filters.page) params = params.set('page', filters.page.toString());
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());

    return this.http.get<any>(`${this.apiUrl}/${storeCode}/items`, { params }).pipe(
      map(res => res.data as PublicCatalogResponse),
      tap({
        next: (data) => {
          this.store.set(data.store);
          this.items.set(data.items);
          this.filterMeta.set(data.filterMeta);
          this.totalCount.set(data.totalCount);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
          this.errorMessage.set(err?.error?.message ?? 'ไม่สามารถดึงข้อมูลรายการสินค้าได้');
        }
      })
    );
  }

  getItemDetail(storeCode: string, itemId: string): Observable<PublicCatalogItem> {
    return this.http.get<any>(`${this.apiUrl}/${storeCode}/items/${itemId}`).pipe(
      map(res => res.data as PublicCatalogItem)
    );
  }
}
