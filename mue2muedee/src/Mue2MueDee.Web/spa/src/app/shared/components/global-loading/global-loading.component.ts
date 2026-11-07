import { Component, DestroyRef, TemplateRef, effect, inject, untracked, viewChild } from '@angular/core';
import { Dialog, DialogRef } from '@angular/cdk/dialog';
import { Overlay } from '@angular/cdk/overlay';
import { LoadingService } from '../../../core/loading/loading.service';
import { DOCUMENT } from '@angular/common';

@Component({
  selector: 'app-global-loading',
  standalone: true,
  templateUrl: './global-loading.component.html',
  styleUrl: './global-loading.component.css'
})
export class GlobalLoadingComponent {
  private readonly loading = inject(LoadingService);
  private readonly dialog = inject(Dialog);
  private readonly overlay = inject(Overlay);
  private readonly document = inject(DOCUMENT);
  private readonly template = viewChild<TemplateRef<unknown>>('loadingTemplate');
  private dialogRef: DialogRef | null = null;
  private readonly blockKeyboard = (event: KeyboardEvent): void => {
    if (!this.loading.isLoading() || event.key === 'Tab') return;
    event.stopImmediatePropagation();
    if (!event.ctrlKey && !event.metaKey && !event.altKey) event.preventDefault();
  };

  constructor() {
    effect(() => {
      const busy = this.loading.isLoading();
      const template = this.template();
      untracked(() => {
        if (busy && template && !this.dialogRef) {
          this.document.addEventListener('keydown', this.blockKeyboard, true);
          this.dialogRef = this.dialog.open(template, {
            disableClose: true, closeOnNavigation: false, autoFocus: '.loading-card', restoreFocus: true,
            ariaLabel: 'กำลังดำเนินการ กรุณารอสักครู่',
            backdropClass: 'global-loading-backdrop', panelClass: 'global-loading-panel',
            width: '20rem', maxWidth: 'calc(100vw - 2rem)',
            scrollStrategy: this.overlay.scrollStrategies.block()
          });
        } else if (!busy) {
          this.close();
        }
      });
    });
    inject(DestroyRef).onDestroy(() => this.close());
  }

  private close(): void {
    this.document.removeEventListener('keydown', this.blockKeyboard, true);
    this.dialogRef?.close();
    this.dialogRef = null;
  }
}
