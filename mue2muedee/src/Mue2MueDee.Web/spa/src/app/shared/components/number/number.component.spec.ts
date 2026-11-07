import { NumberComponent } from './number.component';
import { TestBed } from '@angular/core/testing';
import { FormControl } from '@angular/forms';

describe('NumberComponent currency precision', () => {
  it('keeps an out-of-range value so its validation error can be shown', () => {
    const component = TestBed.createComponent(NumberComponent).componentInstance;
    component.min = 10;
    let formValue: number | null = null;
    component.registerOnChange((value: number | null) => formValue = value);
    const input = document.createElement('input');
    input.value = '0';
    component.onInput({ target: input } as unknown as Event);
    expect(formValue).toBe(0);
    expect(component.validate(new FormControl(formValue))).toEqual({ min: { min: 10, actual: 0 } });
  });

  it('keeps the form value and the two-decimal display consistent', () => {
    const component = TestBed.createComponent(NumberComponent).componentInstance;
    component.currency = true;
    let formValue: number | null = null;
    component.registerOnChange((value: number | null) => formValue = value);
    const input = document.createElement('input');
    input.value = '1,234.567';
    component.onInput({ target: input } as unknown as Event);
    component.onBlur();
    expect(formValue).toBe(1234.56);
    expect(component.displayValue).toBe('1,234.56');
  });
});
