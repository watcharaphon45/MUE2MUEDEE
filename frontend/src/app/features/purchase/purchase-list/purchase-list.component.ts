import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { PurchaseService } from '../purchase.service';
import { PurchaseOrder, PurchaseStatus } from '../purchase.models';
import {
  TextboxComponent,
  SelectComponent,
  StatusBadgeComponent
} from '../../../shared/components';

@Component({
  selector: 'app-purchase-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    TextboxComponent,
    SelectComponent,
    StatusBadgeComponent
  ],
  templateUrl: './purchase-list.component.html',
  styleUrl: './purchase-list.component.css'
})
export class PurchaseListComponent implements OnInit {
  protected readonly purchaseService = inject(PurchaseService);
  protected readonly Math = Math;

  readonly statusOptions = [
    { value: '', label: 'ทุกสถานะ' },
    { value: PurchaseStatus.Appraising.toString(), label: 'กำลังตรวจสภาพ/ประเมินราคา' },
    { value: PurchaseStatus.Offered.toString(), label: 'เสนอราคาแล้ว' },
    { value: PurchaseStatus.Accepted.toString(), label: 'ลูกค้าตกลงรับข้อเสนอ' },
    { value: PurchaseStatus.Paid.toString(), label: 'ชำระเงินแล้ว' },
    { value: PurchaseStatus.Received.toString(), label: 'รับเข้าคลังแล้ว' },
    { value: PurchaseStatus.Rejected.toString(), label: 'ลูกค้ายกเลิก/ปฏิเสธ' }
  ];

  filterForm = new FormGroup({
    search: new FormControl(''),
    status: new FormControl('')
  });

  ngOnInit(): void {
    this.search();
  }

  search(page: number = 1): void {
    const val = this.filterForm.value;
    this.purchaseService.loadPurchases({
      search: val.search || undefined,
      status: val.status ? (parseInt(val.status, 10) as PurchaseStatus) : undefined,
      page,
      pageSize: 15
    }).subscribe();
  }

  resetFilters(): void {
    this.filterForm.reset({ search: '', status: '' });
    this.search(1);
  }

  // Summary Metrics computed from current items
  get pendingAppraisals(): number {
    return this.purchaseService.pagedResult().items.filter(
      p => p.status === PurchaseStatus.Appraising || p.status === PurchaseStatus.Draft
    ).length;
  }

  get needsApprovalCount(): number {
    return this.purchaseService.pagedResult().items.filter(p => p.needsApproval).length;
  }

  get totalAgreedAmount(): number {
    return this.purchaseService.pagedResult().items
      .filter(p => p.status === PurchaseStatus.Accepted || p.status === PurchaseStatus.Paid || p.status === PurchaseStatus.Received)
      .reduce((sum, p) => sum + p.totalAgreedPrice, 0);
  }
}
