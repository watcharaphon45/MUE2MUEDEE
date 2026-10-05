import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError, map } from 'rxjs';
import { AuthUser, LoginResponse, StoreRole, UserStoreInfo } from './auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly apiUrl = '/api/auth';

  // Signals
  readonly currentUser = signal<AuthUser | null>(this.getStoredUser());
  readonly currentStore = signal<UserStoreInfo | null>(this.getStoredStore());
  readonly availableStores = signal<UserStoreInfo[]>(this.getStoredAvailableStores());
  readonly accessToken = signal<string | null>(localStorage.getItem('mue2_access_token'));
  readonly refreshToken = signal<string | null>(localStorage.getItem('mue2_refresh_token'));

  // Computed
  readonly isAuthenticated = computed(() => !!this.accessToken() && !!this.currentUser());
  readonly currentRole = computed<StoreRole | null>(() => this.currentStore()?.role ?? null);
  readonly isOwner = computed(() => this.currentRole() === 'Owner');
  readonly isAdmin = computed(() => this.currentRole() === 'Admin' || this.currentRole() === 'Owner');

  login(usernameOrEmail: string, password: string, storeId?: string): Observable<LoginResponse> {
    const payload = { usernameOrEmail, password, storeId };
    return this.http.post<any>(`${this.apiUrl}/login`, payload).pipe(
      map(res => res.data ? res.data : res),
      tap(data => this.handleAuthSuccess(data))
    );
  }

  switchStore(storeId: string): Observable<LoginResponse> {
    return this.http.post<any>(`${this.apiUrl}/switch-store`, { storeId }).pipe(
      map(res => res.data ? res.data : res),
      tap(data => this.handleAuthSuccess(data))
    );
  }

  logout(): void {
    const rt = this.refreshToken();
    if (rt) {
      this.http.post(`${this.apiUrl}/logout`, { refreshToken: rt }).subscribe({
        error: () => {}
      });
    }

    localStorage.removeItem('mue2_access_token');
    localStorage.removeItem('mue2_refresh_token');
    localStorage.removeItem('mue2_user');
    localStorage.removeItem('mue2_current_store');
    localStorage.removeItem('mue2_stores');

    this.currentUser.set(null);
    this.currentStore.set(null);
    this.availableStores.set([]);
    this.accessToken.set(null);
    this.refreshToken.set(null);

    this.router.navigate(['/login']);
  }

  private handleAuthSuccess(res: LoginResponse): void {
    localStorage.setItem('mue2_access_token', res.accessToken);
    localStorage.setItem('mue2_refresh_token', res.refreshToken);
    localStorage.setItem('mue2_user', JSON.stringify(res.user));
    localStorage.setItem('mue2_stores', JSON.stringify(res.availableStores));

    this.accessToken.set(res.accessToken);
    this.refreshToken.set(res.refreshToken);
    this.currentUser.set(res.user);
    this.availableStores.set(res.availableStores);

    const activeStore = res.availableStores.find(s => s.storeId === res.currentStoreId)
      ?? res.availableStores[0] ?? null;

    if (activeStore) {
      localStorage.setItem('mue2_current_store', JSON.stringify(activeStore));
      this.currentStore.set(activeStore);
    }
  }

  private getStoredUser(): AuthUser | null {
    try {
      const data = localStorage.getItem('mue2_user');
      return data ? JSON.parse(data) : null;
    } catch {
      return null;
    }
  }

  private getStoredStore(): UserStoreInfo | null {
    try {
      const data = localStorage.getItem('mue2_current_store');
      return data ? JSON.parse(data) : null;
    } catch {
      return null;
    }
  }

  private getStoredAvailableStores(): UserStoreInfo[] {
    try {
      const data = localStorage.getItem('mue2_stores');
      return data ? JSON.parse(data) : [];
    } catch {
      return [];
    }
  }
}
