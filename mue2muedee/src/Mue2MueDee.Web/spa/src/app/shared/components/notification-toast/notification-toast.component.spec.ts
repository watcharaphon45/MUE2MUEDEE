import { TestBed } from '@angular/core/testing';
import { NotificationService } from '../../../core/notifications/notification.service';
import { LoadingService } from '../../../core/loading/loading.service';
import { NotificationToastComponent } from './notification-toast.component';

describe('Notification toast', () => {
  it('renders message text safely and supports manual dismissal', () => {
    const fixture = TestBed.createComponent(NotificationToastComponent);
    const notifications = TestBed.inject(NotificationService);
    notifications.error('<img src=x onerror=alert(1)>');
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role=alert]')?.textContent).toContain('<img src=x onerror=alert(1)>');
    expect(fixture.nativeElement.querySelector('img')).toBeNull();
    (fixture.nativeElement.querySelector('.toast-close') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.toast')).toBeNull();
  });

  it('hides notifications while the global loading screen is active', () => {
    const fixture = TestBed.createComponent(NotificationToastComponent);
    TestBed.inject(NotificationService).warning('กรุณาตรวจสอบข้อมูล');
    fixture.detectChanges();
    const finish = TestBed.inject(LoadingService).begin();
    fixture.detectChanges();
    expect((fixture.nativeElement.querySelector('.toast-region') as HTMLElement).hidden).toBe(true);
    finish();
    fixture.detectChanges();
    expect((fixture.nativeElement.querySelector('.toast-region') as HTMLElement).hidden).toBe(false);
  });
});
