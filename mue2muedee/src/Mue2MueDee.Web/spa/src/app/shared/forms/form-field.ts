import { AfterViewInit, ChangeDetectorRef, DestroyRef, Directive, ElementRef, HostBinding, HostListener, Injector, Input, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NgControl, Validators } from '@angular/forms';
import { validationMessage } from './validation-message';

let nextFieldId = 0;

/** Read NgControl after CVA creation to avoid a circular value-accessor dependency. */
@Directive()
export abstract class FormField implements OnInit, AfterViewInit {
  @Input() label?: string;
  @Input() required = false;
  @Input() errorMessage?: string | null;

  readonly fieldId = `form-field-${++nextFieldId}`;
  readonly errorId = `${this.fieldId}-error`;
  private readonly injector = inject(Injector);
  private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly changeDetector = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private ngControl: NgControl | null = null;

  ngOnInit(): void {
    this.ngControl = this.injector.get(NgControl, null, { self: true });
  }

  ngAfterViewInit(): void {
    // FormControlName is registered by the parent form after the CVA's OnInit.
    this.ngControl?.control?.events.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.changeDetector.markForCheck();
    });
    this.changeDetector.markForCheck();
  }

  get validationMessage(): string | null {
    if (this.ngControl?.disabled) return null;
    return this.errorMessage || validationMessage(this.ngControl?.control ?? null, this.label);
  }

  get isRequired(): boolean {
    const control = this.ngControl?.control;
    return this.required || !!control?.hasValidator(Validators.required) || !!control?.hasValidator(Validators.requiredTrue);
  }

  @HostBinding('class.field-invalid')
  get invalid(): boolean { return !!this.validationMessage; }

  @HostListener('focusout', ['$event'])
  onFieldFocusOut(event: FocusEvent): void {
    if (!this.element.nativeElement.contains(event.relatedTarget as Node | null)) {
      this.ngControl?.control?.markAsTouched();
    }
  }
}
