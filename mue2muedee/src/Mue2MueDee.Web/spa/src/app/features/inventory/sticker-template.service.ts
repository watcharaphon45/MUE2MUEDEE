import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, map } from 'rxjs';
import { ApiResponse } from '../../core/models/api-response.model';
import {
  StickerTemplate,
  CreateStickerTemplateRequest,
  UpdateStickerTemplateRequest
} from './sticker-template.models';

@Injectable({
  providedIn: 'root'
})
export class StickerTemplateService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/sticker-templates';

  readonly templates = signal<StickerTemplate[]>([]);
  readonly selectedTemplate = signal<StickerTemplate | null>(null);
  readonly loading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);

  loadTemplates(): Observable<StickerTemplate[]> {
    this.loading.set(true);
    this.errorMessage.set(null);

    return this.http.get<ApiResponse<StickerTemplate[]>>(this.baseUrl).pipe(
      map(res => res.data || []),
      tap({
        next: list => {
          this.templates.set(list);
          this.loading.set(false);
          // Auto select default or first if current selection is not in list
          if (!this.selectedTemplate() || !list.some(t => t.id === this.selectedTemplate()?.id)) {
            const def = list.find(t => t.isDefault) || list[0] || null;
            this.selectedTemplate.set(def);
          }
        },
        error: err => {
          this.errorMessage.set(err.error?.message || 'ไม่สามารถโหลดรูปแบบสติ๊กเกอร์ได้');
          this.loading.set(false);
        }
      })
    );
  }

  getDefault(): Observable<StickerTemplate | null> {
    return this.http.get<ApiResponse<StickerTemplate | null>>(`${this.baseUrl}/default`).pipe(
      map(res => res.data ?? null)
    );
  }

  create(req: CreateStickerTemplateRequest): Observable<StickerTemplate> {
    this.loading.set(true);
    return this.http.post<ApiResponse<StickerTemplate>>(this.baseUrl, req).pipe(
      map(res => res.data),
      tap({
        next: created => {
          this.templates.update(list => {
            const updated = req.isDefault
              ? list.map(t => ({ ...t, isDefault: false }))
              : [...list];
            return [created, ...updated];
          });
          this.selectedTemplate.set(created);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      })
    );
  }

  update(id: string, req: UpdateStickerTemplateRequest): Observable<StickerTemplate> {
    this.loading.set(true);
    return this.http.put<ApiResponse<StickerTemplate>>(`${this.baseUrl}/${id}`, req).pipe(
      map(res => res.data),
      tap({
        next: updated => {
          this.templates.update(list =>
            list.map(t => {
              if (t.id === id) return updated;
              if (req.isDefault) return { ...t, isDefault: false };
              return t;
            })
          );
          if (this.selectedTemplate()?.id === id) {
            this.selectedTemplate.set(updated);
          }
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      })
    );
  }

  duplicate(id: string): Observable<StickerTemplate> {
    this.loading.set(true);
    return this.http.post<ApiResponse<StickerTemplate>>(`${this.baseUrl}/${id}/duplicate`, {}).pipe(
      map(res => res.data),
      tap({
        next: duplicated => {
          this.templates.update(list => [duplicated, ...list]);
          this.selectedTemplate.set(duplicated);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      })
    );
  }

  setDefault(id: string): Observable<void> {
    this.loading.set(true);
    return this.http.put<ApiResponse<void>>(`${this.baseUrl}/${id}/default`, {}).pipe(
      map(() => void 0),
      tap({
        next: () => {
          this.templates.update(list =>
            list.map(t => ({ ...t, isDefault: t.id === id }))
          );
          if (this.selectedTemplate()?.id === id) {
            this.selectedTemplate.update(cur => cur ? { ...cur, isDefault: true } : null);
          }
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      })
    );
  }

  delete(id: string): Observable<void> {
    this.loading.set(true);
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/${id}`).pipe(
      map(() => void 0),
      tap({
        next: () => {
          this.templates.update(list => list.filter(t => t.id !== id));
          if (this.selectedTemplate()?.id === id) {
            const remaining = this.templates();
            this.selectedTemplate.set(remaining.find(t => t.isDefault) || remaining[0] || null);
          }
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      })
    );
  }

  resetDefaults(): Observable<void> {
    this.loading.set(true);
    return this.http.post<ApiResponse<void>>(`${this.baseUrl}/reset-defaults`, {}).pipe(
      map(() => void 0),
      tap({
        next: () => {
          this.loading.set(false);
          this.loadTemplates().subscribe();
        },
        error: () => this.loading.set(false)
      })
    );
  }
}
