import { Component, forwardRef, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-number',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './number.component.html',
  styleUrl: './number.component.css',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => NumberComponent),
      multi: true
    }
  ]
})
export class NumberComponent implements ControlValueAccessor {
  /**
   * If true or passed as attribute `<app-number currency>`, formats as financial money (THB, decimals, commas).
   * If omitted, accepts strict integer / numeric input.
   */
  @Input() set currency(val: boolean | '' | string) {
    this._currency = val === '' || val === true || val === 'true';
  }
  get isCurrencyMode(): boolean {
    return this._currency;
  }
  private _currency: boolean = false;

  @Input() label?: string;
  @Input() placeholder?: string;
  @Input() prefix: string = '฿';
  @Input() icon?: string;
  @Input() min?: number;
  @Input() max?: number;
  @Input() required: boolean = false;
  @Input() errorMessage?: string | null;
  @Input() hint?: string;
  @Input() disabled: boolean = false;

  numericValue: number | null = null;
  displayValue: string = '';
  isFocused: boolean = false;

  onChange: (val: number | null) => void = () => {};
  onTouched: () => void = () => {};

  writeValue(val: any): void {
    if (val !== null && val !== undefined && !isNaN(Number(val))) {
      this.numericValue = Number(val);
      this.displayValue = this.formatDisplay(this.numericValue);
    } else {
      this.numericValue = null;
      this.displayValue = '';
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    let raw = input.value;

    if (this.isCurrencyMode) {
      // Allow numbers and one decimal point
      raw = raw.replace(/[^0-9.]/g, '');
      const parts = raw.split('.');
      if (parts.length > 2) {
        raw = parts[0] + '.' + parts.slice(1).join('');
      }
    } else {
      // Strictly digits
      raw = raw.replace(/[^0-9]/g, '');
    }

    this.displayValue = raw;

    if (raw === '' || raw === '.') {
      this.numericValue = null;
    } else {
      let num = Number(raw);
      if (this.min !== undefined && num < this.min) num = this.min;
      if (this.max !== undefined && num > this.max) num = this.max;
      this.numericValue = num;
    }

    this.onChange(this.numericValue);
  }

  onFocus(): void {
    this.isFocused = true;
    if (this.numericValue !== null) {
      // Show unformatted raw value for easy editing
      this.displayValue = this.numericValue.toString();
    }
  }

  onBlur(): void {
    this.isFocused = false;
    this.onTouched();
    if (this.numericValue !== null) {
      this.displayValue = this.formatDisplay(this.numericValue);
    } else {
      this.displayValue = '';
    }
  }

  private formatDisplay(val: number): string {
    if (this.isCurrencyMode) {
      return val.toLocaleString('th-TH', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
      });
    }
    return val.toLocaleString('th-TH');
  }
}
