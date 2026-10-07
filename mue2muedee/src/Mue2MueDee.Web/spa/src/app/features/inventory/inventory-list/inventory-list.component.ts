import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { InventoryService } from '../inventory.service';
import { MasterDataService } from '../../master-data/master-data.service';
import { InventoryItem, InventoryStatus } from '../inventory.models';
import {
  TextboxComponent,
  NumberComponent,
  SelectComponent,
  StatusBadgeComponent
} from '../../../shared/components';

@Component({
  selector: 'app-inventory-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    TextboxComponent,
    NumberComponent,
    SelectComponent,
    StatusBadgeComponent
  ],
  templateUrl: './inventory-list.component.html',
  styleUrl: './inventory-list.component.css'
})
export class InventoryListComponent implements OnInit {
  protected readonly inventoryService = inject(InventoryService);
  protected readonly masterDataService = inject(MasterDataService);
  protected readonly Math = Math;

  readonly statusOptions = [
    { value: '', label: 'ทุกสถานะ' },
    { value: InventoryStatus.Available.toString(), label: 'พร้อมขาย (ในสต็อก)' },
    { value: InventoryStatus.Reserved.toString(), label: 'จองแล้ว' },
    { value: InventoryStatus.Sold.toString(), label: 'ขายแล้ว' },
    { value: InventoryStatus.PendingInspection.toString(), label: 'รอตรวจเช็คสภาพ' },
    { value: InventoryStatus.Repair.toString(), label: 'ส่งซ่อม/บำรุง' }
  ];

  brandOptions = signal<{ value: string; label: string }[]>([{ value: '', label: 'ทุกแบรนด์' }]);
  categoryOptions = signal<{ value: string; label: string }[]>([{ value: '', label: 'ทุกหมวดหมู่' }]);

  filterForm = new FormGroup({
    search: new FormControl(''),
    brandId: new FormControl(''),
    categoryId: new FormControl(''),
    status: new FormControl(''),
    minPrice: new FormControl<number | null>(null),
    maxPrice: new FormControl<number | null>(null)
  });

  // Image Preview Modal
  previewImageUrl = signal<string | null>(null);
  previewImageTitle = signal<string>('');

  openImagePreview(url: string, title?: string, event?: MouseEvent): void {
    if (event) event.stopPropagation();
    this.previewImageUrl.set(url);
    this.previewImageTitle.set(title || 'รูปภาพสินค้า');
  }

  closeImagePreview(): void {
    this.previewImageUrl.set(null);
  }

  ngOnInit(): void {
    this.loadMasterData();
    this.search();
  }

  loadMasterData(): void {
    this.masterDataService.loadBrands().subscribe(brands => {
      this.brandOptions.set([
        { value: '', label: 'ทุกแบรนด์' },
        ...brands.map(b => ({ value: b.id, label: b.name }))
      ]);
    });

    this.masterDataService.loadCategories().subscribe(categories => {
      this.categoryOptions.set([
        { value: '', label: 'ทุกหมวดหมู่' },
        ...categories.map(c => ({ value: c.id, label: c.name }))
      ]);
    });
  }

  search(page: number = 1): void {
    const val = this.filterForm.value;
    this.inventoryService.loadInventory({
      search: val.search || undefined,
      brandId: val.brandId || undefined,
      categoryId: val.categoryId || undefined,
      status: val.status ? (parseInt(val.status, 10) as InventoryStatus) : undefined,
      minPrice: val.minPrice || undefined,
      maxPrice: val.maxPrice || undefined,
      page,
      pageSize: 15
    }).subscribe();
  }

  resetFilters(): void {
    this.filterForm.reset({
      search: '',
      brandId: '',
      categoryId: '',
      status: '',
      minPrice: null,
      maxPrice: null
    });
    this.search(1);
  }

  // Summary Metrics computed from current items
  get totalAvailableItems(): number {
    return this.inventoryService.pagedResult().items.filter(i => i.status === InventoryStatus.Available).length;
  }

  get totalActualCost(): number {
    return this.inventoryService.pagedResult().items
      .filter(i => i.status === InventoryStatus.Available)
      .reduce((sum, i) => sum + i.actualCost, 0);
  }

  get totalSellingPrice(): number {
    return this.inventoryService.pagedResult().items
      .filter(i => i.status === InventoryStatus.Available)
      .reduce((sum, i) => sum + i.sellingPrice, 0);
  }

  get totalExpectedProfit(): number {
    return this.totalSellingPrice - this.totalActualCost;
  }
}
