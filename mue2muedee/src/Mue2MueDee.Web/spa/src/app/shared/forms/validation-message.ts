import { AbstractControl } from '@angular/forms';

/** Messages shared by every form field. Validation rules remain on the form. */
export function validationMessage(control: AbstractControl | null, label?: string): string | null {
  if (!control || control.disabled || !control.invalid || !control.touched) return null;

  const errors = control.errors;
  if (!errors) return null;
  const field = label?.replace(/\s*\*\s*$/, '').trim() || 'ข้อมูลนี้';
  if (errors['required'] || errors['whitespace']) return `กรุณาระบุ${field}`;
  if (errors['email']) return 'กรุณาระบุอีเมลให้ถูกต้อง เช่น name@example.com';
  if (errors['minlength']) return `${field}ต้องมีอย่างน้อย ${errors['minlength'].requiredLength} ตัวอักษร`;
  if (errors['maxlength']) return `${field}ต้องไม่เกิน ${errors['maxlength'].requiredLength} ตัวอักษร`;
  if (errors['min']) return `${field}ต้องไม่น้อยกว่า ${errors['min'].min}`;
  if (errors['max']) return `${field}ต้องไม่เกิน ${errors['max'].max}`;
  if (errors['pattern']) return `รูปแบบ${field}ไม่ถูกต้อง`;
  if (typeof errors['server'] === 'string') return errors['server'];
  return `กรุณาตรวจสอบ${field}ให้ถูกต้อง`;
}
