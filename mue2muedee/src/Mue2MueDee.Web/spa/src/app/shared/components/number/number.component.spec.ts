import { NumberComponent } from './number.component';

describe('NumberComponent currency precision', () => {
  it('keeps the form value and the two-decimal display consistent', () => {
    const component = new NumberComponent();
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
