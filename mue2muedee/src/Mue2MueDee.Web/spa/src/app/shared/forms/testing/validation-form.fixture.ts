import { Component, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TextboxComponent } from '../../components/textbox/textbox.component';
import { NumberComponent } from '../../components/number/number.component';
import { TextareaComponent } from '../../components/textarea/textarea.component';
import { SelectComponent } from '../../components/select/select.component';
import { DatepickerComponent } from '../../components/datepicker/datepicker.component';
import { CheckboxComponent } from '../../components/checkbox/checkbox.component';
import { ImageUploaderComponent } from '../../components/image-uploader/image-uploader.component';
import { MultiImageUploaderComponent } from '../../components/multi-image-uploader/multi-image-uploader.component';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, TextboxComponent, NumberComponent, TextareaComponent, SelectComponent,
    DatepickerComponent, CheckboxComponent, ImageUploaderComponent, MultiImageUploaderComponent],
  templateUrl: './validation-form.fixture.html',
  styleUrl: './validation-form.fixture.css'
})
export class ValidationFormFixture {
  readonly form = new FormGroup({
    text: new FormControl('', [Validators.required, Validators.minLength(3)]),
    amount: new FormControl<number | null>(null, [Validators.required, Validators.min(10), Validators.max(100)]),
    notes: new FormControl('', Validators.required),
    selection: new FormControl('', Validators.required),
    date: new FormControl('', Validators.required),
    accepted: new FormControl(false, Validators.requiredTrue),
    image: new FormControl('', Validators.required),
    images: new FormControl<string[]>([], Validators.required)
  });
  readonly standalone = new FormControl('', [Validators.required, Validators.email]);
  explicitError = signal<string | null>(null);
}
