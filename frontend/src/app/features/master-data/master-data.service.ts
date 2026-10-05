import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, map } from 'rxjs';
import { ApiResponse } from '../../core/models/api-response.model';
import {
  Brand,
  Category,
  ConditionGrade,
  Customer,
  Product,
  CreateProductRequest,
  CreateCustomerRequest
} from './master-data.models';

@Injectable({
  providedIn: 'root'
})
export class MasterDataService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api';

  readonly brands = signal<Brand[]>([]);
  readonly categories = signal<Category[]>([]);
  readonly conditionGrades = signal<ConditionGrade[]>([]);
  readonly products = signal<Product[]>([]);
  readonly customers = signal<Customer[]>([]);
  readonly loading = signal<boolean>(false);

  // Brands
  loadBrands(): Observable<Brand[]> {
    this.loading.set(true);
    return this.http.get<ApiResponse<Brand[]>>(`${this.baseUrl}/master-data/brands`).pipe(
      map(res => res.data),
      tap(data => {
        this.brands.set(data);
        this.loading.set(false);
      })
    );
  }

  // Categories
  loadCategories(): Observable<Category[]> {
    this.loading.set(true);
    return this.http.get<ApiResponse<Category[]>>(`${this.baseUrl}/master-data/categories`).pipe(
      map(res => res.data),
      tap(data => {
        this.categories.set(data);
        this.loading.set(false);
      })
    );
  }

  // Condition Grades
  loadConditionGrades(): Observable<ConditionGrade[]> {
    this.loading.set(true);
    return this.http.get<ApiResponse<ConditionGrade[]>>(`${this.baseUrl}/master-data/condition-grades`).pipe(
      map(res => res.data),
      tap(data => {
        this.conditionGrades.set(data);
        this.loading.set(false);
      })
    );
  }

  // Products
  loadProducts(brandId?: string, categoryId?: string, search?: string): Observable<Product[]> {
    this.loading.set(true);
    const params: Record<string, string> = {};
    if (brandId) params['brandId'] = brandId;
    if (categoryId) params['categoryId'] = categoryId;
    if (search) params['search'] = search;

    return this.http.get<ApiResponse<Product[]>>(`${this.baseUrl}/master-data/products`, { params }).pipe(
      map(res => res.data),
      tap(data => {
        this.products.set(data);
        this.loading.set(false);
      })
    );
  }

  createProduct(req: CreateProductRequest): Observable<Product> {
    return this.http.post<ApiResponse<Product>>(`${this.baseUrl}/master-data/products`, req).pipe(
      map(res => res.data),
      tap(newProduct => {
        this.products.update(list => [...list, newProduct]);
      })
    );
  }

  // Customers
  loadCustomers(search?: string): Observable<Customer[]> {
    this.loading.set(true);
    const params: Record<string, string> = {};
    if (search) params['search'] = search;

    return this.http.get<ApiResponse<Customer[]>>(`${this.baseUrl}/customers`, { params }).pipe(
      map(res => res.data),
      tap(data => {
        this.customers.set(data);
        this.loading.set(false);
      })
    );
  }

  createCustomer(req: CreateCustomerRequest): Observable<Customer> {
    return this.http.post<ApiResponse<Customer>>(`${this.baseUrl}/customers`, req).pipe(
      map(res => res.data),
      tap(newCustomer => {
        this.customers.update(list => [newCustomer, ...list]);
      })
    );
  }
}
