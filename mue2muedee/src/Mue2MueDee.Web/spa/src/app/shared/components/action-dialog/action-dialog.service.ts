import { Injectable, inject } from '@angular/core';
import { Dialog, DialogRef } from '@angular/cdk/dialog';
import { NavigationStart, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { firstValueFrom } from 'rxjs';
import { ActionDialogComponent, ActionDialogData } from './action-dialog.component';

export interface ActionDialogOptions {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  tone?: 'primary' | 'danger';
}

@Injectable({ providedIn: 'root' })
export class ActionDialogService {
  private readonly dialog = inject(Dialog);
  private activeDialog: DialogRef<boolean> | null = null;

  constructor() {
    inject(Router).events.pipe(takeUntilDestroyed()).subscribe(event => {
      if (event instanceof NavigationStart) this.activeDialog?.close(false);
    });
  }

  confirm(options: ActionDialogOptions): Promise<boolean> {
    return this.open(options, true);
  }

  async notify(options: ActionDialogOptions): Promise<void> {
    await this.open({ ...options, confirmText: options.confirmText ?? 'ตกลง' }, false);
  }

  private async open(options: ActionDialogOptions, showCancel: boolean): Promise<boolean> {
    // A repeated click must not create a second action waiting for confirmation.
    if (this.activeDialog) return false;
    const ref = this.dialog.open<boolean, ActionDialogData>(ActionDialogComponent, {
      data: { ...options, confirmText: options.confirmText ?? 'ยืนยัน', cancelText: options.cancelText ?? 'ยกเลิก', tone: options.tone ?? 'primary', showCancel },
      width: '28rem', maxWidth: 'calc(100vw - 2rem)',
      ariaLabelledBy: 'action-dialog-title', ariaDescribedBy: 'action-dialog-message',
      autoFocus: showCancel ? '.dialog-cancel' : '.dialog-confirm', restoreFocus: true,
      backdropClass: 'action-dialog-backdrop', closeOnNavigation: true
    });
    this.activeDialog = ref;
    try {
      return (await firstValueFrom(ref.closed, { defaultValue: false })) === true;
    } finally {
      if (this.activeDialog === ref) this.activeDialog = null;
    }
  }
}
