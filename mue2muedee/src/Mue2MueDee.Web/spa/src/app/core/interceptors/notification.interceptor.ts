import { HttpErrorResponse, HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { tap } from 'rxjs';
import { NotificationService } from '../notifications/notification.service';
import { ApiResponse } from '../models/api-response.model';

export const notificationInterceptor: HttpInterceptorFn = (request, next) => {
  const notifications = inject(NotificationService);
  const mutation = !['GET', 'HEAD', 'OPTIONS'].includes(request.method);
  const path = new URL(request.url, 'http://application.local').pathname;
  return next(request).pipe(tap({
    next: event => {
      if (!(event instanceof HttpResponse)) return;
      const body = event.body as Partial<ApiResponse<{ needsApproval?: boolean }>> | null;
      if (!body || typeof body !== 'object') return;
      const message = typeof body.message === 'string' && body.message.trim() ? body.message : 'ทำรายการสำเร็จ';
      if (body.success === false) notifications.warning(message === 'ทำรายการสำเร็จ' ? 'ไม่สามารถทำรายการได้ กรุณาตรวจสอบข้อมูล' : message);
      else if (mutation && !path.endsWith('/auth/refresh')) {
        if (body.data?.needsApproval === true) notifications.warning(message);
        else if (path.endsWith('/auth/switch-store')) notifications.saveSuccessForReload(message);
        else notifications.success(message);
      }
    },
    error: (error: HttpErrorResponse) => {
      const message = typeof error.error?.message === 'string' && error.error.message.trim() ? error.error.message :
        error.status === 0 ? 'ไม่สามารถเชื่อมต่อเซิร์ฟเวอร์ได้ กรุณาตรวจสอบการเชื่อมต่อแล้วลองใหม่' :
        error.status >= 400 && error.status < 500 ? 'ไม่สามารถทำรายการได้ กรุณาตรวจสอบข้อมูลหรือสิทธิ์การใช้งาน' :
        'เกิดข้อผิดพลาดของระบบ กรุณาลองใหม่อีกครั้ง';
      if (error.status >= 400 && error.status < 500) notifications.warning(message);
      else notifications.error(message);
    }
  }));
};
