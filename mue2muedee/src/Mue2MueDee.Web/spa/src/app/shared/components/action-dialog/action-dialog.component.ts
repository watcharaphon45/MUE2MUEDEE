import { Component, inject } from '@angular/core';
import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';

export interface ActionDialogData {
  title: string;
  message: string;
  confirmText: string;
  cancelText: string;
  tone: 'primary' | 'danger';
  showCancel: boolean;
}

@Component({
  selector: 'app-action-dialog',
  standalone: true,
  templateUrl: './action-dialog.component.html',
  styleUrl: './action-dialog.component.css'
})
export class ActionDialogComponent {
  readonly data = inject<ActionDialogData>(DIALOG_DATA);
  readonly dialogRef = inject<DialogRef<boolean>>(DialogRef);
}
