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
  CreateBrandRequest,
  UpdateBrandRequest,
  CreateCategoryRequest,
  UpdateCategoryRequest,
  CreateConditionGradeRequest,
  UpdateConditionGradeRequest,
  CreateProductRequest,
  UpdateProductRequest,
  CreateCustomerRequest,
  UpdateCustomerRequest
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

  // ==========================================
  // BRANDS CRUD
  // ==========================================

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

  createBrand(req: CreateBrandRequest): Observable<Brand> {
    return this.http.post<ApiResponse<Brand>>(`${this.baseUrl}/master-data/brands`, req).pipe(
      map(res => res.data),
      tap(newBrand => {
        this.brands.update(list => [...list, newBrand]);
      })
    );
  }

  updateBrand(id: string, req: UpdateBrandRequest): Observable<Brand> {
    return this.http.put<ApiResponse<Brand>>(`${this.baseUrl}/master-data/brands/${id}`, req).pipe(
      map(res => res.data),
      tap(updated => {
        this.brands.update(list => list.map(b => b.id === id ? updated : b));
      })
    );
  }

  deleteBrand(id: string): Observable<void> {
    return this.http.delete<ApiResponse<any>>(`${this.baseUrl}/master-data/brands/${id}`).pipe(
      map(() => {}),
      tap(() => {
        this.brands.update(list => list.filter(b => b.id !== id));
      })
    );
  }

  // ==========================================
  // CATEGORIES CRUD
  // ==========================================

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

  createCategory(req: CreateCategoryRequest): Observable<Category> {
    return this.http.post<ApiResponse<Category>>(`${this.baseUrl}/master-data/categories`, req).pipe(
      map(res => res.data),
      tap(newCat => {
        this.categories.update(list => [...list, newCat]);
      })
    );
  }

  updateCategory(id: string, req: UpdateCategoryRequest): Observable<Category> {
    return this.http.put<ApiResponse<Category>>(`${this.baseUrl}/master-data/categories/${id}`, req).pipe(
      map(res => res.data),
      tap(updated => {
        this.categories.update(list => list.map(c => c.id === id ? updated : c));
      })
    );
  }

  deleteCategory(id: string): Observable<void> {
    return this.http.delete<ApiResponse<any>>(`${this.baseUrl}/master-data/categories/${id}`).pipe(
      map(() => {}),
      tap(() => {
        this.categories.update(list => list.filter(c => c.id !== id));
      })
    );
  }

  // ==========================================
  // CONDITION GRADES CRUD
  // ==========================================

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

  createConditionGrade(req: CreateConditionGradeRequest): Observable<ConditionGrade> {
    return this.http.post<ApiResponse<ConditionGrade>>(`${this.baseUrl}/master-data/condition-grades`, req).pipe(
      map(res => res.data),
      tap(newGrade => {
        this.conditionGrades.update(list => [...list, newGrade]);
      })
    );
  }

  updateConditionGrade(id: string, req: UpdateConditionGradeRequest): Observable<ConditionGrade> {
    return this.http.put<ApiResponse<ConditionGrade>>(`${this.baseUrl}/master-data/condition-grades/${id}`, req).pipe(
      map(res => res.data),
      tap(updated => {
        this.conditionGrades.update(list => list.map(g => g.id === id ? updated : g));
      })
    );
  }

  deleteConditionGrade(id: string): Observable<void> {
    return this.http.delete<ApiResponse<any>>(`${this.baseUrl}/master-data/condition-grades/${id}`).pipe(
      map(() => {}),
      tap(() => {
        this.conditionGrades.update(list => list.filter(g => g.id !== id));
      })
    );
  }

  // ==========================================
  // PRODUCTS CRUD
  // ==========================================

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

  updateProduct(id: string, req: UpdateProductRequest): Observable<Product> {
    return this.http.put<ApiResponse<Product>>(`${this.baseUrl}/master-data/products/${id}`, req).pipe(
      map(res => res.data),
      tap(updated => {
        this.products.update(list => list.map(p => p.id === id ? updated : p));
      })
    );
  }

  deleteProduct(id: string): Observable<void> {
    return this.http.delete<ApiResponse<any>>(`${this.baseUrl}/master-data/products/${id}`).pipe(
      map(() => {}),
      tap(() => {
        this.products.update(list => list.filter(p => p.id !== id));
      })
    );
  }

  // ==========================================
  // CUSTOMERS CRUD
  // ==========================================

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

  updateCustomer(id: string, req: UpdateCustomerRequest): Observable<Customer> {
    return this.http.put<ApiResponse<Customer>>(`${this.baseUrl}/customers/${id}`, req).pipe(
      map(res => res.data),
      tap(updated => {
        this.customers.update(list => list.map(c => c.id === id ? updated : c));
      })
    );
  }

  deleteCustomer(id: string): Observable<void> {
    return this.http.delete<ApiResponse<any>>(`${this.baseUrl}/customers/${id}`).pipe(
      map(() => {}),
      tap(() => {
        this.customers.update(list => list.filter(c => c.id !== id));
      })
    );
  }
}
