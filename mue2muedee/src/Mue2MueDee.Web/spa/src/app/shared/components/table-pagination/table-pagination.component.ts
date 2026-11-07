import { Component, OnChanges, SimpleChanges, computed, input, output, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SelectComponent } from '../select/select.component';

export interface TablePageChange {
  page: number;
  pageSize: number;
}

function defaultRowKey<T>(item: T): unknown {
  return item !== null && typeof item === 'object' && 'id' in item ? item.id : item;
}

@Component({
  selector: 'app-table-pagination',
  standalone: true,
  imports: [ReactiveFormsModule, SelectComponent],
  templateUrl: './table-pagination.component.html',
  styleUrl: './table-pagination.component.css'
})
export class TablePaginationComponent<T> implements OnChanges {
  readonly items = input<readonly T[]>([]);
  readonly totalCount = input<number | null>(null);
  readonly page = input(1);
  readonly pageSize = input(15);
  readonly disabled = input(false);
  readonly label = input('ตารางข้อมูล');
  readonly rowKey = input<(item: T) => unknown>(defaultRowKey);
  readonly paginationChange = output<TablePageChange>();

  private readonly localPage = signal(1);
  private readonly localSize = signal(15);
  readonly sizeControl = new FormControl('15', { nonNullable: true });
  readonly sizeOptions = [10, 15, 20, 25, 50, 100].map(size => ({ value: String(size), label: `${size} แถว` }));
  readonly serverMode = computed(() => this.totalCount() !== null);
  readonly count = computed(() => Math.max(0, this.totalCount() ?? this.items().length));
  readonly size = computed(() => Math.max(1, this.serverMode() ? this.pageSize() : this.localSize()));
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.count() / this.size())));
  readonly currentPage = computed(() => Math.min(this.totalPages(), Math.max(1, this.serverMode() ? this.page() : this.localPage())));
  readonly startIndex = computed(() => (this.currentPage() - 1) * this.size());
  readonly rangeStart = computed(() => this.count() === 0 ? 0 : this.startIndex() + 1);
  readonly rangeEnd = computed(() => Math.min(this.startIndex() + this.size(), this.count()));
  readonly pagedItems = computed(() => this.serverMode() ? this.items() : this.items().slice(this.startIndex(), this.startIndex() + this.size()));
  readonly visiblePages = computed(() => {
    const start = Math.max(1, Math.min(this.currentPage() - 2, this.totalPages() - 4));
    return Array.from({ length: Math.min(5, this.totalPages()) }, (_, index) => start + index);
  });

  constructor() {
    this.sizeControl.valueChanges.pipe(takeUntilDestroyed()).subscribe(value => this.changeSize(Number(value)));
  }

  ngOnChanges(changes: SimpleChanges): void {
    const rows = changes['items'];
    if (!this.serverMode() && rows) {
      const previous = rows.previousValue as readonly T[] | undefined;
      const current = this.items();
      const key = this.rowKey();
      if (!previous || previous.length !== current.length || current.some((item, index) => key(item) !== key(previous[index]))) {
        this.localPage.set(1);
      }
    }
    this.sizeControl.setValue(String(this.size()), { emitEvent: false });
    if (this.disabled()) this.sizeControl.disable({ emitEvent: false });
    else this.sizeControl.enable({ emitEvent: false });
  }

  changePage(page: number): void {
    if (this.disabled() || page < 1 || page > this.totalPages() || page === this.currentPage()) return;
    if (this.serverMode()) this.paginationChange.emit({ page, pageSize: this.size() });
    else this.localPage.set(page);
  }

  changeSize(pageSize: number): void {
    if (this.disabled() || !this.sizeOptions.some(option => Number(option.value) === pageSize)) return;
    if (this.serverMode()) this.paginationChange.emit({ page: 1, pageSize });
    else {
      this.localSize.set(pageSize);
      this.localPage.set(1);
    }
  }
}
