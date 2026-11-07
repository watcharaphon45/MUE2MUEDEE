import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { ValidationFormFixture } from './testing/validation-form.fixture';

describe('Shared form field validation', () => {
  beforeEach(() => TestBed.configureTestingModule({
    imports: [ValidationFormFixture], providers: [provideHttpClient()]
  }));

  function setup() {
    const fixture = TestBed.createComponent(ValidationFormFixture);
    fixture.detectChanges();
    return { fixture, component: fixture.componentInstance, root: fixture.nativeElement as HTMLElement };
  }

  it('keeps untouched forms quiet and exposes every invalid field after submit', async () => {
    const { fixture, root } = setup();
    expect(root.querySelectorAll('.field-invalid').length).toBe(0);
    root.querySelector('form')!.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    await fixture.whenStable();
    fixture.detectChanges();
    expect(root.querySelectorAll('.field-invalid').length).toBe(8);
    expect(root.querySelectorAll('[role="status"]').length).toBe(8);
    for (const field of root.querySelectorAll('.field-invalid')) {
      const input = field.querySelector('[aria-invalid="true"]')!;
      expect(input).not.toBeNull();
      expect(input.getAttribute('aria-describedby')).toBe(field.querySelector('[role="status"]')!.id);
    }
  });

  it('shows validation on blur and clears it immediately after correction', async () => {
    const { fixture, component, root } = setup();
    const input = root.querySelector('app-textbox input') as HTMLInputElement;
    input.dispatchEvent(new FocusEvent('blur'));
    await fixture.whenStable();
    fixture.detectChanges();
    expect(root.textContent).toContain('กรุณาระบุชื่อ');
    input.value = 'ก';
    input.dispatchEvent(new Event('input', { bubbles: true }));
    await fixture.whenStable();
    fixture.detectChanges();
    expect(root.textContent).toContain('อย่างน้อย 3 ตัวอักษร');
    component.form.controls.text.setValue('ชื่อถูกต้อง');
    await fixture.whenStable();
    fixture.detectChanges();
    expect(root.querySelector('app-textbox')!.classList.contains('field-invalid')).toBe(false);
  });

  it('updates every CVA after programmatic correction, disable and reset', async () => {
    const { fixture, component, root } = setup();
    component.form.markAllAsTouched();
    await fixture.whenStable();
    fixture.detectChanges();
    component.form.setValue({ text: 'ครบถ้วน', amount: 20, notes: 'บันทึก', selection: 'one',
      date: '2026-11-07', accepted: true, image: '/image.jpg', images: ['/image.jpg'] });
    await fixture.whenStable();
    fixture.detectChanges();
    expect(root.querySelectorAll('.field-invalid').length).toBe(0);
    component.form.reset();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(root.querySelectorAll('.field-invalid').length).toBe(0);
    component.form.markAllAsTouched();
    component.form.disable();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(root.querySelectorAll('.field-invalid').length).toBe(0);
  });

  it('shows min/max and standalone email errors under the relevant field', async () => {
    const { fixture, component, root } = setup();
    component.form.controls.amount.setValue(9);
    component.form.controls.amount.markAsTouched();
    component.standalone.setValue('invalid-email');
    component.standalone.markAsTouched();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(root.querySelector('app-number')!.textContent).toContain('ไม่น้อยกว่า 10');
    expect(root.querySelector('app-textbox:last-child')!.textContent).toContain('อีเมลให้ถูกต้อง');
    component.form.controls.amount.setValue(101);
    await fixture.whenStable();
    fixture.detectChanges();
    expect(root.querySelector('app-number')!.textContent).toContain('ต้องไม่เกิน 100');
  });

  it('preserves an explicitly supplied error message', async () => {
    const { fixture, component, root } = setup();
    component.explicitError.set('ชื่อนี้ถูกใช้แล้ว');
    fixture.detectChanges();
    expect(root.querySelector('app-textbox')!.textContent).toContain('ชื่อนี้ถูกใช้แล้ว');
  });
});
