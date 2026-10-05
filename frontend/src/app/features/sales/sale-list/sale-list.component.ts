import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { SaleService } from '../sale.service';
import { SaleStatus } from '../sale.models';
import { StatusBadgeComponent } from '../../../shared/components';

@Component({
  selector: 'app-sale-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    StatusBadgeComponent
  ],
  templateUrl: './sale-list.component.html',
  styleUrl: './sale-list.component.css'
})
export class SaleListComponent implements OnInit {
  protected readonly saleService = inject(SaleService);

  searchControl = new FormControl('');
  statusFilter = new FormControl<string>('ALL');

  readonly statusTabs = [
    { value: 'ALL', label: 'ทั้งหมด' },
    { value: SaleStatus.Completed.toString(), label: 'เสร็จสมบูรณ์' },
    { value: SaleStatus.Cancelled.toString(), label: 'ยกเลิกแล้ว' }
  ];

  ngOnInit(): void {
    this.loadData();
    this.searchControl.valueChanges.subscribe(() => this.loadData());
    this.statusFilter.valueChanges.subscribe(() => this.loadData());
  }

  loadData(): void {
    const s = this.statusFilter.value;
    const status = s && s !== 'ALL' ? (parseInt(s, 10) as SaleStatus) : undefined;

    this.saleService.loadSales({
      search: this.searchControl.value || undefined,
      status,
      page: 1,
      pageSize: 25
    }).subscribe();
  }
}
