import { Component, forwardRef, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, ControlValueAccessor, NG_VALIDATORS, NG_VALUE_ACCESSOR, ValidationErrors, Validator, Validators } from '@angular/forms';
import { FormField } from '../../forms/form-field';

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
    },
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => NumberComponent),
      multi: true
    }
  ]
})
export class NumberComponent extends FormField implements ControlValueAccessor, Validator, OnChanges {
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

  @Input() placeholder?: string;
  @Input() prefix: string = '฿';
  @Input() icon?: string;
  @Input() min?: number;
  @Input() max?: number;
  @Input() hint?: string;
  @Input() disabled: boolean = false;

  numericValue: number | null = null;
  displayValue: string = '';
  isFocused: boolean = false;

  onChange: (val: number | null) => void = () => {};
  onTouched: () => void = () => {};
  private onValidatorChange: () => void = () => {};

  validate(control: AbstractControl): ValidationErrors | null {
    const minError = this.min === undefined ? null : Validators.min(Number(this.min))(control);
    const maxError = this.max === undefined ? null : Validators.max(Number(this.max))(control);
    return minError || maxError;
  }

  registerOnValidatorChange(fn: () => void): void { this.onValidatorChange = fn; }
  ngOnChanges(): void { this.onValidatorChange(); }

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
      if (parts.length > 1) {
        raw = parts[0] + '.' + parts.slice(1).join('').slice(0, 2);
      }
    } else {
      // Strictly digits
      raw = raw.replace(/[^0-9]/g, '');
    }

    this.displayValue = raw;

    if (raw === '' || raw === '.') {
      this.numericValue = null;
    } else {
      this.numericValue = Number(raw);
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
