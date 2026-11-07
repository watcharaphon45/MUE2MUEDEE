import { TestBed } from '@angular/core/testing';
import { LoadingService } from '../loading/loading.service';
import { NotificationService } from './notification.service';

describe('Notification lifetime and queue', () => {
  beforeEach(() => { vi.useFakeTimers(); sessionStorage.clear(); });
  afterEach(() => { TestBed.resetTestingModule(); sessionStorage.clear(); vi.useRealTimers(); });
  const setup = () => {
    const notifications = TestBed.inject(NotificationService);
    TestBed.tick();
    return { notifications, loading: TestBed.inject(LoadingService) };
  };

  it('deduplicates the same message and expires each notification by its severity', () => {
    const { notifications } = setup();
    const first = notifications.success('บันทึกสำเร็จ');
    expect(notifications.success('บันทึกสำเร็จ')).toBe(first);
    notifications.warning('ข้อมูลซ้ำ');
    notifications.error('เชื่อมต่อไม่ได้');
    expect(notifications.items().length).toBe(3);
    vi.advanceTimersByTime(4500);
    expect(notifications.items().map(item => item.kind)).toEqual(['warning', 'error']);
    vi.advanceTimersByTime(2500);
    expect(notifications.items().map(item => item.kind)).toEqual(['error']);
    vi.advanceTimersByTime(2000);
    expect(notifications.items()).toEqual([]);
  });

  it('waits for all requests and keeps only the final success of a multi-step action', () => {
    const { notifications, loading } = setup();
    const first = loading.begin();
    const second = loading.begin();
    notifications.success('สร้างใบรับซื้อแล้ว');
    notifications.success('เสนอราคาแล้ว');
    notifications.warning('ต้องอนุมัติราคา');
    TestBed.tick();
    vi.advanceTimersByTime(30000);
    expect(notifications.items()).toEqual([]);
    first();
    TestBed.tick();
    expect(notifications.items()).toEqual([]);
    second();
    TestBed.tick();
    expect(notifications.items().map(item => item.message)).toEqual(['เสนอราคาแล้ว', 'ต้องอนุมัติราคา']);
  });

  it('keeps a message readable until both hover and keyboard focus have left', () => {
    const { notifications } = setup();
    const id = notifications.warning('กรุณาตรวจสอบข้อมูล');
    notifications.pause(id, 'hover');
    notifications.pause(id, 'focus');
    vi.advanceTimersByTime(10000);
    notifications.resume(id, 'hover');
    vi.advanceTimersByTime(10000);
    expect(notifications.items().length).toBe(1);
    notifications.resume(id, 'focus');
    vi.advanceTimersByTime(7000);
    expect(notifications.items()).toEqual([]);
  });

  it('pauses visible notifications while loading blocks the screen', () => {
    const { notifications, loading } = setup();
    notifications.success('บันทึกสำเร็จ');
    vi.advanceTimersByTime(4000);
    const finish = loading.begin();
    TestBed.tick();
    vi.advanceTimersByTime(15000);
    expect(notifications.items().length).toBe(1);
    finish();
    TestBed.tick();
    vi.advanceTimersByTime(4499);
    expect(notifications.items().length).toBe(1);
    vi.advanceTimersByTime(1);
    expect(notifications.items()).toEqual([]);
  });

  it('limits visible messages and displays the next pending message after dismissal', () => {
    const { notifications } = setup();
    for (let index = 0; index < 5; index++) notifications.warning(`เงื่อนไข ${index}`);
    expect(notifications.items().length).toBe(4);
    notifications.dismiss(notifications.items()[0].id);
    expect(notifications.items().map(item => item.message)).toEqual(['เงื่อนไข 1', 'เงื่อนไข 2', 'เงื่อนไข 3', 'เงื่อนไข 4']);
  });

  it('restores a store switch notification once after a full page reload', () => {
    const { notifications } = setup();
    notifications.saveSuccessForReload('สลับร้านค้าเรียบร้อยแล้ว');
    TestBed.resetTestingModule();
    const restored = setup().notifications;
    expect(restored.items()[0].message).toBe('สลับร้านค้าเรียบร้อยแล้ว');
    TestBed.resetTestingModule();
    expect(setup().notifications.items()).toEqual([]);
  });
});
