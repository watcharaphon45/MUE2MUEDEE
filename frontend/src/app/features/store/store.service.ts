import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { AddUserRequest, StoreDetails, StoreUser, UpdateStoreRequest } from './store.models';
import { ApiResponse } from '../../core/models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class StoreService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/stores';

  getCurrentStore(): Observable<StoreDetails> {
    return this.http.get<ApiResponse<StoreDetails>>(`${this.apiUrl}/current`).pipe(
      map(res => res.data)
    );
  }

  updateCurrentStore(data: Partial<UpdateStoreRequest>): Observable<StoreDetails> {
    return this.http.put<ApiResponse<StoreDetails>>(`${this.apiUrl}/current`, data).pipe(
      map(res => res.data)
    );
  }

  getStoreUsers(): Observable<StoreUser[]> {
    return this.http.get<ApiResponse<StoreUser[]>>(`${this.apiUrl}/users`).pipe(
      map(res => res.data)
    );
  }

  addUserToStore(data: AddUserRequest): Observable<StoreUser> {
    return this.http.post<ApiResponse<StoreUser>>(`${this.apiUrl}/users`, data).pipe(
      map(res => res.data)
    );
  }

  updateUserRole(userId: string, role: string, isActive: boolean): Observable<StoreUser> {
    return this.http.put<ApiResponse<StoreUser>>(`${this.apiUrl}/users/${userId}/role`, { role, isActive }).pipe(
      map(res => res.data)
    );
  }
}
