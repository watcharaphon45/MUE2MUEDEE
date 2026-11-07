import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, ValidatorFn } from '@angular/forms';

/** Connect a reactive control to existing signal-driven previews and presets. */
export function formValue<T>(initialValue: T, validators: ValidatorFn[]) {
  const control = new FormControl<T>(initialValue, { nonNullable: true, validators });
  const value = toSignal(control.valueChanges, { initialValue });
  // An empty numeric input remains invalid in the control while preview math uses a safe fallback.
  return Object.assign(() => {
    const current = value();
    return control.invalid ? initialValue : current ?? initialValue;
  }, {
    control,
    set: (next: T) => control.setValue(next)
  });
}
