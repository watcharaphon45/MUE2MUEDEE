import { DestroyRef, Injectable, effect, inject, signal, untracked } from '@angular/core';
import { LoadingService } from '../loading/loading.service';

export type NotificationKind = 'success' | 'warning' | 'error';
export interface NotificationItem {
  id: number;
  kind: NotificationKind;
  message: string;
}
type PauseReason = 'hover' | 'focus';
const reloadKey = 'mue2_notification_after_reload';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly loading = inject(LoadingService);
  private readonly visible = signal<NotificationItem[]>([]);
  readonly items = this.visible.asReadonly();
  private pending: NotificationItem[] = [];
  private sequence = 0;
  private readonly timers = new Map<number, ReturnType<typeof setTimeout>>();
  private readonly paused = new Map<number, Set<PauseReason>>();
  private readonly durations = { success: 4500, warning: 7000, error: 9000 };

  constructor() {
    this.restoreAfterReload();
    effect(() => {
      const busy = this.loading.isLoading();
      untracked(() => {
        if (busy) {
          this.stopTimers();
          this.paused.clear();
        } else this.drain();
      });
    });
    inject(DestroyRef).onDestroy(() => this.stopTimers());
  }

  success(message: string): number { return this.show('success', message); }
  warning(message: string): number { return this.show('warning', message); }
  error(message: string): number { return this.show('error', message); }
  validation(): number { return this.warning('กรุณากรอกข้อมูลที่จำเป็นให้ครบถ้วนและถูกต้อง'); }

  dismiss(id: number): void {
    this.stopTimer(id);
    this.paused.delete(id);
    this.visible.update(items => items.filter(item => item.id !== id));
    this.pending = this.pending.filter(item => item.id !== id);
    this.drain();
  }

  pause(id: number, reason: PauseReason): void {
    const reasons = this.paused.get(id) ?? new Set<PauseReason>();
    reasons.add(reason);
    this.paused.set(id, reasons);
    this.stopTimer(id);
  }

  resume(id: number, reason: PauseReason): void {
    this.paused.get(id)?.delete(reason);
    const item = this.visible().find(item => item.id === id);
    if (item) this.startTimer(item);
  }

  saveSuccessForReload(message: string): void {
    try { sessionStorage.setItem(reloadKey, JSON.stringify({ message, at: Date.now() })); }
    catch { this.success(message); }
  }

  private show(kind: NotificationKind, text: string): number {
    const message = text.trim();
    if (!message) return 0;
    const existing = [...this.visible(), ...this.pending].find(item => item.kind === kind && item.message === message);
    if (existing) return existing.id;
    const item = { id: ++this.sequence, kind, message };
    // One action can save and reload several resources; show its final success message.
    if (kind === 'success' && this.loading.isLoading()) this.pending = this.pending.filter(item => item.kind !== 'success');
    this.pending.push(item);
    this.drain();
    return item.id;
  }

  private drain(): void {
    if (this.loading.isLoading()) return;
    const items = [...this.visible()];
    while (items.length < 4 && this.pending.length) items.push(this.pending.shift()!);
    this.visible.set(items);
    items.forEach(item => this.startTimer(item));
  }

  private startTimer(item: NotificationItem): void {
    if (this.loading.isLoading() || this.timers.has(item.id) || this.paused.get(item.id)?.size) return;
    this.timers.set(item.id, setTimeout(() => this.dismiss(item.id), this.durations[item.kind]));
  }

  private stopTimer(id: number): void {
    const timer = this.timers.get(id);
    if (timer !== undefined) clearTimeout(timer);
    this.timers.delete(id);
  }

  private stopTimers(): void {
    this.timers.forEach(timer => clearTimeout(timer));
    this.timers.clear();
  }

  private restoreAfterReload(): void {
    try {
      const saved = sessionStorage.getItem(reloadKey);
      sessionStorage.removeItem(reloadKey);
      if (!saved) return;
      const value = JSON.parse(saved);
      if (typeof value.message === 'string' && typeof value.at === 'number' && Date.now() - value.at >= 0 && Date.now() - value.at < 15000) {
        this.pending.push({ id: ++this.sequence, kind: 'success', message: value.message });
      }
    } catch { /* Storage is optional. */ }
  }
}
