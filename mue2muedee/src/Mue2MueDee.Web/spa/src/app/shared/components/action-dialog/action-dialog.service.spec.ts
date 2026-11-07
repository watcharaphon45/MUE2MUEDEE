import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { App } from '../../../app';
import { ActionDialogService } from './action-dialog.service';

describe('Action dialog', () => {
  function setup() {
    TestBed.configureTestingModule({ providers: [provideRouter([{ path: 'next', component: App }])] });
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    return { fixture, dialogs: TestBed.inject(ActionDialogService) };
  }
  const options = { title: 'ยืนยันรายการ', message: 'รายละเอียดรายการที่ต้องยืนยัน' };

  it('resolves true only after the confirm button is clicked and prevents duplicate dialogs', async () => {
    const { fixture, dialogs } = setup();
    const result = dialogs.confirm(options);
    expect(await dialogs.confirm(options)).toBe(false);
    await fixture.whenStable();
    expect(document.querySelectorAll('app-action-dialog').length).toBe(1);
    expect(document.querySelector('[role=dialog]')?.getAttribute('aria-labelledby')).toBe('action-dialog-title');
    (document.querySelector('.dialog-confirm') as HTMLButtonElement).click();
    expect(await result).toBe(true);
  });

  it('resolves false on cancel', async () => {
    const { fixture, dialogs } = setup();
    const result = dialogs.confirm(options);
    await fixture.whenStable();
    (document.querySelector('.dialog-cancel') as HTMLButtonElement).click();
    expect(await result).toBe(false);
    expect(document.querySelector('app-action-dialog')).toBeNull();
  });

  it('treats backdrop clicks as cancellation', async () => {
    const { fixture, dialogs } = setup();
    const result = dialogs.confirm(options);
    await fixture.whenStable();
    (document.querySelector('.cdk-overlay-backdrop') as HTMLElement).click();
    expect(await result).toBe(false);
  });

  it('treats Escape as cancellation', async () => {
    const { fixture, dialogs } = setup();
    const result = dialogs.confirm(options);
    await fixture.whenStable();
    document.body.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', keyCode: 27, bubbles: true }));
    expect(await result).toBe(false);
  });

  it('cancels a pending action when navigating away', async () => {
    const { dialogs } = setup();
    const result = dialogs.confirm(options);
    await TestBed.inject(Router).navigateByUrl('/next');
    expect(await result).toBe(false);
  });

  it('shows a single acknowledgement button for a notification', async () => {
    const { fixture, dialogs } = setup();
    const result = dialogs.notify(options);
    await fixture.whenStable();
    expect(document.querySelector('.dialog-cancel')).toBeNull();
    expect(document.querySelector('.dialog-confirm')?.textContent).toBe('ตกลง');
    (document.querySelector('.dialog-confirm') as HTMLButtonElement).click();
    await result;
  });
});
