import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  Output,
  ViewChild,
  forwardRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormsModule } from '@angular/forms';

export interface SelectOption {
  value: any;
  label: string;
  disabled?: boolean;
}

@Component({
  selector: 'app-select',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './select.component.html',
  styleUrl: './select.component.css',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectComponent),
      multi: true
    }
  ]
})
export class SelectComponent implements ControlValueAccessor {
  @Input() label?: string;
  @Input() placeholder: string = 'กรุณาเลือก...';
  @Input() options: SelectOption[] = [];
  @Input() required: boolean = false;
  @Input() errorMessage?: string | null;
  @Input() hint?: string;
  @Input() disabled: boolean = false;
  @Input() icon?: string;
  @Input() clearable: boolean = true;
  @Input() searchable: boolean = true;
  @Input() panelClass: string = '';

  @Input() set value(val: any) {
    this.writeValue(val);
  }

  @Output() selectionChange = new EventEmitter<SelectOption | null>();
  @Output() valueChange = new EventEmitter<any>();

  @ViewChild('triggerContainer') triggerContainer?: ElementRef<HTMLElement>;
  @ViewChild('mainInput') mainInputElement?: ElementRef<HTMLInputElement>;
  @ViewChild('dropdownPanel') dropdownPanel?: ElementRef<HTMLElement>;

  selectedValue: any = '';
  isOpen: boolean = false;
  isSearching: boolean = false;
  searchText: string = '';
  highlightedIndex: number = -1;
  openUpwards: boolean = false;

  onChange: (val: any) => void = () => {};
  onTouched: () => void = () => {};

  constructor(private readonly elementRef: ElementRef) {}

  // ControlValueAccessor Implementation
  writeValue(val: any): void {
    this.selectedValue = val ?? '';
    this.isSearching = false;
    this.searchText = '';
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled;
    if (isDisabled && this.isOpen) {
      this.closeDropdown();
    }
  }

  // Check if a valid, non-empty value is currently selected
  get hasValue(): boolean {
    return (
      this.selectedValue !== null &&
      this.selectedValue !== undefined &&
      this.selectedValue !== ''
    );
  }

  // UX Rule: EXACTLY ONE action icon on the right
  // If hasValue -> show only clear icon (×)
  // If !hasValue -> show only dropdown arrow (▼ or ▲)
  get showClearIcon(): boolean {
    if (this.disabled) return false;
    if (!this.clearable) return false;
    return this.hasValue || (this.isOpen && !!this.searchText.trim());
  }

  // Text displayed in the main input
  get displayInputText(): string {
    if (this.isSearching) {
      return this.searchText;
    }
    return this.selectedOption ? this.selectedOption.label : '';
  }

  // Dynamic placeholder
  get inputPlaceholder(): string {
    if (this.isOpen && !this.selectedOption) {
      return 'ค้นหา...';
    }
    return this.placeholder || 'กรุณาเลือก...';
  }

  // Filtered options based on real-time search (Thai/English bilingual support)
  get filteredOptions(): SelectOption[] {
    if (!this.searchable || !this.isSearching || !this.searchText.trim()) {
      return this.options || [];
    }
    const query = this.searchText.trim().toLowerCase();
    return (this.options || []).filter(opt => {
      const labelStr = (opt.label ?? '').toString().toLowerCase();
      const valueStr = (opt.value ?? '').toString().toLowerCase();
      return labelStr.includes(query) || valueStr.includes(query);
    });
  }

  // Find the selected option object
  get selectedOption(): SelectOption | undefined {
    if (!this.options || this.options.length === 0) {
      return undefined;
    }

    if (this.selectedValue === '' || this.selectedValue === null || this.selectedValue === undefined) {
      return this.options.find(opt => opt.value === '' || opt.value === null || opt.value === undefined);
    }

    return this.options.find(opt => this.isSelected(opt));
  }

  isSelected(opt: SelectOption): boolean {
    if (this.selectedValue === opt.value) {
      return true;
    }
    if (
      this.selectedValue !== null &&
      this.selectedValue !== undefined &&
      this.selectedValue !== '' &&
      opt.value !== null &&
      opt.value !== undefined &&
      opt.value !== ''
    ) {
      return String(this.selectedValue) === String(opt.value);
    }
    return false;
  }

  toggleDropdown(): void {
    if (this.disabled) return;
    if (this.isOpen) {
      this.closeDropdown();
    } else {
      this.openDropdown();
    }
  }

  openDropdown(): void {
    if (this.disabled) return;

    if (this.triggerContainer?.nativeElement) {
      const rect = this.triggerContainer.nativeElement.getBoundingClientRect();
      const spaceBelow = window.innerHeight - rect.bottom;
      this.openUpwards = spaceBelow < 260 && rect.top > 260;
    }

    this.isOpen = true;
    this.isSearching = false;
    this.searchText = '';

    const currentIdx = this.filteredOptions.findIndex(opt => this.isSelected(opt));
    this.highlightedIndex = currentIdx !== -1 ? currentIdx : this.findFirstEnabledIndex();

    setTimeout(() => {
      if (this.mainInputElement?.nativeElement) {
        this.mainInputElement.nativeElement.focus();
        if (this.selectedOption) {
          this.mainInputElement.nativeElement.select();
        }
      }
      this.scrollHighlightedIntoView();
    }, 30);
  }

  closeDropdown(): void {
    if (!this.isOpen) return;
    this.isOpen = false;
    this.isSearching = false;
    this.searchText = '';
    this.highlightedIndex = -1;
    this.onTouched();
  }

  onContainerClick(event: MouseEvent): void {
    if (this.disabled) return;

    // If dropdown is closed, open it and focus input
    if (!this.isOpen) {
      this.openDropdown();
    } else {
      // If already open, ensure main input retains focus
      this.mainInputElement?.nativeElement.focus();
    }
  }

  onInputFocus(): void {
    if (this.disabled) return;
    if (!this.isOpen) {
      this.openDropdown();
    }
  }

  onInputChange(event: Event): void {
    if (!this.searchable) return;
    const target = event.target as HTMLInputElement;
    this.searchText = target.value;
    this.isSearching = true;

    if (!this.isOpen) {
      this.isOpen = true;
    }

    this.highlightedIndex = this.findFirstEnabledIndex();
    this.scrollHighlightedIntoView();
  }

  onArrowClick(event: MouseEvent): void {
    event.stopPropagation();
    event.preventDefault();
    if (this.disabled) return;
    this.toggleDropdown();
  }

  onClearClick(event: MouseEvent): void {
    event.stopPropagation();
    event.preventDefault();
    if (this.disabled) return;

    const emptyValue = this.options.some(o => o.value === '') ? '' : null;
    this.selectedValue = emptyValue;
    this.searchText = '';
    this.isSearching = false;

    this.onChange(emptyValue);
    this.valueChange.emit(emptyValue);
    this.selectionChange.emit(null);
    this.onTouched();

    if (this.mainInputElement?.nativeElement) {
      this.mainInputElement.nativeElement.value = '';
      this.mainInputElement.nativeElement.focus();
    }
  }

  selectOption(opt: SelectOption, event?: Event): void {
    if (opt.disabled || this.disabled) return;
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }

    this.selectedValue = opt.value;
    this.isSearching = false;
    this.searchText = '';

    this.onChange(this.selectedValue);
    this.valueChange.emit(this.selectedValue);
    this.selectionChange.emit(opt);

    this.closeDropdown();
    this.mainInputElement?.nativeElement.focus();
  }

  setHighlightedIndex(index: number): void {
    if (this.filteredOptions[index]?.disabled) return;
    this.highlightedIndex = index;
  }

  onBlur(): void {
    setTimeout(() => {
      if (!this.elementRef.nativeElement.contains(document.activeElement)) {
        if (this.isOpen) {
          this.closeDropdown();
        }
      }
    }, 150);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      if (this.isOpen) {
        this.closeDropdown();
      }
    }
  }

  onKeyDown(event: KeyboardEvent): void {
    if (this.disabled) return;

    switch (event.key) {
      case 'Enter':
        event.preventDefault();
        if (this.isOpen) {
          if (this.highlightedIndex >= 0 && this.highlightedIndex < this.filteredOptions.length) {
            const opt = this.filteredOptions[this.highlightedIndex];
            if (!opt.disabled) {
              this.selectOption(opt);
            }
          }
        } else {
          this.openDropdown();
        }
        break;

      case 'ArrowDown':
        event.preventDefault();
        if (!this.isOpen) {
          this.openDropdown();
        } else {
          this.navigateHighlight(1);
        }
        break;

      case 'ArrowUp':
        event.preventDefault();
        if (!this.isOpen) {
          this.openDropdown();
        } else {
          this.navigateHighlight(-1);
        }
        break;

      case 'Home':
        if (this.isOpen) {
          event.preventDefault();
          this.highlightedIndex = this.findFirstEnabledIndex();
          this.scrollHighlightedIntoView();
        }
        break;

      case 'End':
        if (this.isOpen) {
          event.preventDefault();
          this.highlightedIndex = this.findLastEnabledIndex();
          this.scrollHighlightedIntoView();
        }
        break;

      case 'Escape':
        if (this.isOpen) {
          event.preventDefault();
          event.stopPropagation();
          this.closeDropdown();
          this.mainInputElement?.nativeElement.focus();
        }
        break;

      case 'Tab':
        if (this.isOpen) {
          this.closeDropdown();
        }
        break;
    }
  }

  private navigateHighlight(step: number): void {
    const list = this.filteredOptions;
    if (list.length === 0) return;

    let next = this.highlightedIndex + step;
    while (next >= 0 && next < list.length && list[next]?.disabled) {
      next += step;
    }

    if (next >= 0 && next < list.length) {
      this.highlightedIndex = next;
      this.scrollHighlightedIntoView();
    }
  }

  private findFirstEnabledIndex(): number {
    return this.filteredOptions.findIndex(opt => !opt.disabled);
  }

  private findLastEnabledIndex(): number {
    for (let i = this.filteredOptions.length - 1; i >= 0; i--) {
      if (!this.filteredOptions[i].disabled) return i;
    }
    return -1;
  }

  private scrollHighlightedIntoView(): void {
    if (!this.dropdownPanel?.nativeElement) return;
    const items = this.dropdownPanel.nativeElement.querySelectorAll('[role="option"]');
    if (this.highlightedIndex >= 0 && this.highlightedIndex < items.length) {
      const el = items[this.highlightedIndex] as HTMLElement;
      el?.scrollIntoView({ block: 'nearest' });
    }
  }
}
