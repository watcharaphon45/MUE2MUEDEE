import { Component, inject } from '@angular/core';
import { NotificationKind, NotificationService } from '../../../core/notifications/notification.service';
import { LoadingService } from '../../../core/loading/loading.service';

@Component({
  selector: 'app-notification-toast',
  standalone: true,
  templateUrl: './notification-toast.component.html',
  styleUrl: './notification-toast.component.css'
})
export class NotificationToastComponent {
  protected readonly notifications = inject(NotificationService);
  protected readonly loading = inject(LoadingService);
  readonly titles: Record<NotificationKind, string> = { success: 'ทำรายการสำเร็จ', warning: 'กรุณาตรวจสอบ', error: 'เกิดข้อผิดพลาด' };
  readonly icons: Record<NotificationKind, string> = { success: 'check_circle', warning: 'warning_amber', error: 'error_outline' };
}
