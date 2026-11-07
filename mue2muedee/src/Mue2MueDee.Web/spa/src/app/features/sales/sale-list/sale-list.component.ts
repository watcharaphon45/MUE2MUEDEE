import { TablePaginationComponent, TablePageChange } from '../../../shared/components/table-pagination/table-pagination.component';
import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { SaleService } from '../sale.service';
import { SaleStatus } from '../sale.models';
import { StatusBadgeComponent, DatepickerComponent } from '../../../shared/components';

@Component({
  selector: 'app-sale-list',
  standalone: true,
  imports: [
    TablePaginationComponent,
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    StatusBadgeComponent,
    DatepickerComponent
  ],
  templateUrl: './sale-list.component.html',
  styleUrl: './sale-list.component.css'
})
export class SaleListComponent implements OnInit {
  readonly tablePageSize = signal(25);

  changePagination(change: TablePageChange): void {
    this.tablePageSize.set(change.pageSize);
    this.loadData(change.page);
  }

  protected readonly saleService = inject(SaleService);

  searchControl = new FormControl('');
  statusFilter = new FormControl<string>('ALL');
  startDateControl = new FormControl<string>('');
  endDateControl = new FormControl<string>('');

  readonly statusTabs = [
    { value: 'ALL', label: 'ทั้งหมด' },
    { value: SaleStatus.Completed.toString(), label: 'เสร็จสมบูรณ์' },
    { value: SaleStatus.Cancelled.toString(), label: 'ยกเลิกแล้ว' }
  ];

  ngOnInit(): void {
    this.loadData();
    this.searchControl.valueChanges.subscribe(() => this.loadData());
    this.statusFilter.valueChanges.subscribe(() => this.loadData());
    this.startDateControl.valueChanges.subscribe(() => this.loadData());
    this.endDateControl.valueChanges.subscribe(() => this.loadData());
  }

  loadData(page: number = 1): void {
    const s = this.statusFilter.value;
    const status = s && s !== 'ALL' ? (parseInt(s, 10) as SaleStatus) : undefined;

    this.saleService.loadSales({
      search: this.searchControl.value || undefined,
      status,
      startDate: this.startDateControl.value || undefined,
      endDate: this.endDateControl.value || undefined,
      page,
      pageSize: this.tablePageSize()
    }).subscribe();
  }
}
