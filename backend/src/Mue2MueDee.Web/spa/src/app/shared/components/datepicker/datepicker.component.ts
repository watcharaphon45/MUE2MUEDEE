import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnInit,
  Output,
  ViewChild,
  forwardRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormsModule } from '@angular/forms';

export interface CalendarDay {
  date: Date;
  dateString: string; // 'YYYY-MM-DD'
  dayNumber: number;
  isCurrentMonth: boolean;
  isToday: boolean;
  isSelected: boolean;
  isInRange: boolean;
  isRangeStart: boolean;
  isRangeEnd: boolean;
  isDisabled: boolean;
}

const THAI_MONTHS_SHORT = [
  'ม.ค.', 'ก.พ.', 'มี.ค.', 'เม.ย.', 'พ.ค.', 'มิ.ย.',
  'ก.ค.', 'ส.ค.', 'ก.ย.', 'ต.ค.', 'พ.ย.', 'ธ.ค.'
];

const THAI_MONTHS_FULL = [
  'มกราคม', 'กุมภาพันธ์', 'มีนาคม', 'เมษายน', 'พฤษภาคม', 'มิถุนายน',
  'กรกฎาคม', 'สิงหาคม', 'กันยายน', 'ตุลาคม', 'พฤศจิกายน', 'ธันวาคม'
];

@Component({
  selector: 'app-datepicker',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './datepicker.component.html',
  styleUrl: './datepicker.component.css',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DatepickerComponent),
      multi: true
    }
  ]
})
export class DatepickerComponent implements OnInit, ControlValueAccessor {
  @Input() label?: string;
  @Input() placeholder: string = 'เลือกวันที่...';
  @Input() required: boolean = false;
  @Input() disabled: boolean = false;
  @Input() errorMessage?: string | null;
  @Input() hint?: string;
  @Input() icon: string = 'calendar_today';
  @Input() clearable: boolean = true;
  @Input() minDate?: string; // YYYY-MM-DD
  @Input() maxDate?: string; // YYYY-MM-DD
  @Input() displayFormat: 'thai' | 'iso' | 'short' = 'thai';
  @Input() rangeStartDate?: string;
  @Input() rangeEndDate?: string;
  @Input() panelClass: string = '';

  @Input() set value(val: string) {
    this.writeValue(val);
  }

  @Output() dateChange = new EventEmitter<string>();

  @ViewChild('triggerContainer') triggerContainer?: ElementRef<HTMLElement>;
  @ViewChild('calendarPanel') calendarPanel?: ElementRef<HTMLElement>;

  selectedDate: string = ''; // YYYY-MM-DD
  isOpen: boolean = false;
  openUpwards: boolean = false;

  // View state
  viewDate: Date = new Date();
  viewMode: 'days' | 'months' | 'years' = 'days';
  yearPageStart: number = 2020;

  readonly weekDays = ['อา', 'จ', 'อ', 'พ', 'พฤ', 'ศ', 'ส'];
  readonly thaiMonthsShort = THAI_MONTHS_SHORT;
  readonly thaiMonthsFull = THAI_MONTHS_FULL;

  calendarDays: CalendarDay[] = [];

  onChange: (val: string) => void = () => {};
  onTouched: () => void = () => {};

  constructor(private readonly elementRef: ElementRef) {}

  ngOnInit(): void {
    this.initViewDate();
    this.generateCalendarDays();
  }

  // ControlValueAccessor Implementation
  writeValue(val: any): void {
    if (val instanceof Date) {
      this.selectedDate = this.formatToDateString(val);
    } else if (typeof val === 'string' && val.trim().length > 0) {
      // Clean string (e.g., if ISO string with T, slice 0..10)
      this.selectedDate = val.includes('T') ? val.slice(0, 10) : val.trim();
    } else {
      this.selectedDate = '';
    }

    this.initViewDate();
    this.generateCalendarDays();
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
      this.closeCalendar();
    }
  }

  get hasValue(): boolean {
    return !!this.selectedDate;
  }

  get displayValue(): string {
    if (!this.selectedDate) return '';
    return this.formatDisplay(this.selectedDate);
  }

  get currentMonthName(): string {
    return this.thaiMonthsFull[this.viewDate.getMonth()] || '';
  }

  get currentYearNumber(): number {
    return this.viewDate.getFullYear();
  }

  get yearList(): number[] {
    const list: number[] = [];
    for (let y = this.yearPageStart; y < this.yearPageStart + 12; y++) {
      list.push(y);
    }
    return list;
  }

  // Handle open/close click
  toggleCalendar(event?: MouseEvent): void {
    if (this.disabled) return;
    if (event) event.stopPropagation();

    if (this.isOpen) {
      this.closeCalendar();
    } else {
      this.openCalendar();
    }
  }

  openCalendar(): void {
    if (this.disabled) return;
    this.initViewDate();
    this.generateCalendarDays();
    this.viewMode = 'days';
    this.checkPanelPlacement();
    this.isOpen = true;
  }

  closeCalendar(): void {
    this.isOpen = false;
    this.viewMode = 'days';
    this.onTouched();
  }

  private checkPanelPlacement(): void {
    if (!this.triggerContainer?.nativeElement) return;
    const rect = this.triggerContainer.nativeElement.getBoundingClientRect();
    const spaceBelow = window.innerHeight - rect.bottom;
    const spaceAbove = rect.top;

    // Calendar height approx 320px
    this.openUpwards = spaceBelow < 330 && spaceAbove > 330;
  }

  private initViewDate(): void {
    if (this.selectedDate) {
      const parts = this.selectedDate.split('-');
      if (parts.length === 3) {
        const y = parseInt(parts[0], 10);
        const m = parseInt(parts[1], 10) - 1;
        const d = parseInt(parts[2], 10);
        this.viewDate = new Date(y, m, d);
        this.yearPageStart = Math.floor(y / 12) * 12;
        return;
      }
    }
    this.viewDate = new Date();
    this.yearPageStart = Math.floor(this.viewDate.getFullYear() / 12) * 12;
  }

  // Calendar generation
  generateCalendarDays(): void {
    const year = this.viewDate.getFullYear();
    const month = this.viewDate.getMonth();

    const todayStr = this.formatToDateString(new Date());
    const firstDayOfMonth = new Date(year, month, 1);
    const lastDayOfMonth = new Date(year, month + 1, 0);

    const startDayOfWeek = firstDayOfMonth.getDay(); // 0 = Sun, 1 = Mon ...
    const daysInMonth = lastDayOfMonth.getDate();

    const days: CalendarDay[] = [];

    // Days from previous month
    for (let i = startDayOfWeek - 1; i >= 0; i--) {
      const d = new Date(year, month, -i);
      days.push(this.createCalendarDay(d, false, todayStr));
    }

    // Days in current month
    for (let i = 1; i <= daysInMonth; i++) {
      const d = new Date(year, month, i);
      days.push(this.createCalendarDay(d, true, todayStr));
    }

    // Days in next month to fill complete grid of 35 or 42 cells
    const remaining = 35 - days.length >= 0 ? 35 - days.length : 42 - days.length;
    for (let i = 1; i <= remaining; i++) {
      const d = new Date(year, month + 1, i);
      days.push(this.createCalendarDay(d, false, todayStr));
    }

    this.calendarDays = days;
  }

  private createCalendarDay(date: Date, isCurrentMonth: boolean, todayStr: string): CalendarDay {
    const dateString = this.formatToDateString(date);

    let isDisabled = false;
    if (this.minDate && dateString < this.minDate) isDisabled = true;
    if (this.maxDate && dateString > this.maxDate) isDisabled = true;

    const isSelected = this.selectedDate === dateString;
    const isToday = dateString === todayStr;

    let isInRange = false;
    let isRangeStart = false;
    let isRangeEnd = false;

    if (this.rangeStartDate && this.rangeEndDate) {
      isRangeStart = dateString === this.rangeStartDate;
      isRangeEnd = dateString === this.rangeEndDate;
      isInRange = dateString > this.rangeStartDate && dateString < this.rangeEndDate;
    } else if (this.rangeStartDate && !this.rangeEndDate && this.selectedDate) {
      isInRange = dateString > this.rangeStartDate && dateString < this.selectedDate;
    }

    return {
      date,
      dateString,
      dayNumber: date.getDate(),
      isCurrentMonth,
      isToday,
      isSelected,
      isInRange,
      isRangeStart,
      isRangeEnd,
      isDisabled
    };
  }

  // User interactions
  selectDay(day: CalendarDay, event: MouseEvent): void {
    event.stopPropagation();
    if (day.isDisabled) return;

    this.selectedDate = day.dateString;
    this.onChange(this.selectedDate);
    this.dateChange.emit(this.selectedDate);
    this.generateCalendarDays();
    this.closeCalendar();
  }

  selectToday(event: MouseEvent): void {
    event.stopPropagation();
    const todayStr = this.formatToDateString(new Date());
    if ((this.minDate && todayStr < this.minDate) || (this.maxDate && todayStr > this.maxDate)) {
      return;
    }
    this.selectedDate = todayStr;
    this.viewDate = new Date();
    this.onChange(this.selectedDate);
    this.dateChange.emit(this.selectedDate);
    this.generateCalendarDays();
    this.closeCalendar();
  }

  clearValue(event: MouseEvent): void {
    event.stopPropagation();
    if (this.disabled) return;
    this.selectedDate = '';
    this.onChange('');
    this.dateChange.emit('');
    this.generateCalendarDays();
  }

  // Navigation
  prevMonth(event: MouseEvent): void {
    event.stopPropagation();
    this.viewDate = new Date(this.viewDate.getFullYear(), this.viewDate.getMonth() - 1, 1);
    this.generateCalendarDays();
  }

  nextMonth(event: MouseEvent): void {
    event.stopPropagation();
    this.viewDate = new Date(this.viewDate.getFullYear(), this.viewDate.getMonth() + 1, 1);
    this.generateCalendarDays();
  }

  setViewMode(mode: 'days' | 'months' | 'years', event?: MouseEvent): void {
    if (event) event.stopPropagation();
    this.viewMode = mode;
    if (mode === 'years') {
      this.yearPageStart = Math.floor(this.viewDate.getFullYear() / 12) * 12;
    }
  }

  selectMonth(monthIndex: number, event: MouseEvent): void {
    event.stopPropagation();
    this.viewDate = new Date(this.viewDate.getFullYear(), monthIndex, 1);
    this.viewMode = 'days';
    this.generateCalendarDays();
  }

  selectYear(year: number, event: MouseEvent): void {
    event.stopPropagation();
    this.viewDate = new Date(year, this.viewDate.getMonth(), 1);
    this.viewMode = 'months';
  }

  prevYearPage(event: MouseEvent): void {
    event.stopPropagation();
    this.yearPageStart -= 12;
  }

  nextYearPage(event: MouseEvent): void {
    event.stopPropagation();
    this.yearPageStart += 12;
  }

  // Format Helpers
  private formatToDateString(d: Date): string {
    const year = d.getFullYear();
    const month = (d.getMonth() + 1).toString().padStart(2, '0');
    const day = d.getDate().toString().padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private formatDisplay(dateStr: string): string {
    const parts = dateStr.split('-');
    if (parts.length !== 3) return dateStr;
    const year = parseInt(parts[0], 10);
    const month = parseInt(parts[1], 10);
    const day = parseInt(parts[2], 10);

    if (isNaN(year) || isNaN(month) || isNaN(day)) return dateStr;

    if (this.displayFormat === 'iso') {
      return dateStr;
    }

    if (this.displayFormat === 'short') {
      return `${day.toString().padStart(2, '0')}/${month.toString().padStart(2, '0')}/${year}`;
    }

    // Default 'thai'
    const monthName = THAI_MONTHS_SHORT[month - 1] || `${month}`;
    return `${day.toString().padStart(2, '0')} ${monthName} ${year}`;
  }

  // Close when clicking outside
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.isOpen) return;
    const target = event.target as HTMLElement;
    if (!this.elementRef.nativeElement.contains(target)) {
      this.closeCalendar();
    }
  }
}
