import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { of } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { ActionDialogService } from '../../../shared/components/action-dialog/action-dialog.service';
import { StickerTemplateService } from '../sticker-template.service';
import { StickerDesignerModalComponent } from './sticker-designer-modal.component';

describe('Sticker designer validation', () => {
  const create = vi.fn();
  const update = vi.fn();

  beforeEach(() => {
    create.mockClear();
    update.mockClear();
    TestBed.configureTestingModule({
      imports: [StickerDesignerModalComponent],
      providers: [
        { provide: AuthService, useValue: { currentStore: () => null } },
        { provide: ActionDialogService, useValue: { confirm: vi.fn() } },
        { provide: StickerTemplateService, useValue: { loadTemplates: () => of([]), templates: signal([]), create, update } }
      ]
    });
  });

  async function setup() {
    const fixture = TestBed.createComponent(StickerDesignerModalComponent);
    fixture.componentRef.setInput('isOpen', true);
    fixture.detectChanges();
    await fixture.whenStable();
    return { fixture, component: fixture.componentInstance, root: fixture.nativeElement as HTMLElement };
  }

  it('rejects an empty template name before calling the API', async () => {
    const { fixture, component, root } = await setup();
    component.templateName.control.setValue('');
    component.saveTemplate();
    fixture.detectChanges();
    expect(create).not.toHaveBeenCalled();
    expect(update).not.toHaveBeenCalled();
    expect(root.querySelector('#sticker-templateName-error')).not.toBeNull();
    expect(root.querySelector('input[aria-describedby="sticker-templateName-error"]')!.getAttribute('aria-invalid')).toBe('true');
  });

  it('validates ranges without letting invalid values break preview calculations', async () => {
    const { component } = await setup();
    component.columnsCount.control.setValue(0);
    component.saveTemplate();
    expect(component.columnsCount.control.hasError('min')).toBe(true);
    expect(component.columnsCount()).toBe(1);
    expect(Number.isFinite(component.totalRequiredWidthMm())).toBe(true);
    expect(create).not.toHaveBeenCalled();
  });

  it('excludes hidden paper dimensions on a roll and restores valid defaults for a sheet', async () => {
    const { fixture, component } = await setup();
    expect(component.paperHeightMm.control.disabled).toBe(true);
    expect(component.rowsCount.control.disabled).toBe(true);
    expect(component.settingsForm.valid).toBe(true);
    component.isContinuousRoll.set(false);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(component.paperHeightMm.control.value).toBe(297);
    expect(component.rowsCount.control.value).toBe(8);
    expect(component.settingsForm.valid).toBe(true);
  });

  it('shows a width constraint under the paper field and clears it after correction', async () => {
    const { fixture, component, root } = await setup();
    component.labelWidthMm.control.setValue(60);
    component.saveTemplate();
    fixture.detectChanges();
    expect(create).not.toHaveBeenCalled();
    expect(root.querySelector('#sticker-paperWidthMm-error')!.textContent).toContain('ความกว้างกระดาษไม่พอ');
    component.paperWidthMm.control.setValue(70);
    fixture.detectChanges();
    expect(root.querySelector('#sticker-paperWidthMm-error')).toBeNull();
  });
});
