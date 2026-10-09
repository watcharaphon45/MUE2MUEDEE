import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryService } from '../inventory.service';
import {
  InventoryItem,
  InventoryStatus,
  StockMovement,
  CostType,
  CostBreakdown,
  PricingRecommendation,
  PriceHistory
} from '../inventory.models';
import {
  TextboxComponent,
  NumberComponent,
  TextareaComponent,
  SelectComponent,
  StatusBadgeComponent,
  MultiImageUploaderComponent
} from '../../../shared/components';
import { StickerDesignerModalComponent } from '../sticker-designer-modal/sticker-designer-modal.component';

@Component({
  selector: 'app-inventory-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    TextboxComponent,
    NumberComponent,
    TextareaComponent,
    SelectComponent,
    StatusBadgeComponent,
    MultiImageUploaderComponent,
    StickerDesignerModalComponent
  ],
  templateUrl: './inventory-detail.component.html',
  styleUrl: './inventory-detail.component.css'
})
export class InventoryDetailComponent implements OnInit {
  protected readonly inventoryService = inject(InventoryService);
  private readonly route = inject(ActivatedRoute);

  itemId = signal<string>('');
  activeTab = signal<'overview' | 'costs' | 'pricing' | 'timeline'>('overview');
  showAddCostModal = signal<boolean>(false);
  showUpdatePriceModal = signal<boolean>(false);
  showChangeStatusModal = signal<boolean>(false);
  showManageImagesModal = signal<boolean>(false);
  showImagePreview = signal<boolean>(false);
  showStickerModal = signal<boolean>(false);
  activeImageIndex = signal<number>(0);
  isSubmitting = signal<boolean>(false);
  feedbackMessage = signal<{ type: 'success' | 'error'; text: string } | null>(null);

  // Images Form Control
  imagesControl = new FormControl<string[]>([], { nonNullable: true });

  // Live margin adjustment for pricing recommendation
  targetMarginControl = new FormControl<number>(15, [Validators.required, Validators.min(1), Validators.max(90)]);

  // Add Item Cost Form
  costItemForm = new FormGroup({
    costType: new FormControl<string>(CostType.Repair.toString(), [Validators.required]),
    amount: new FormControl<number | null>(null, [Validators.required, Validators.min(1)]),
    description: new FormControl('', [Validators.required])
  });

  // Update Price Form
  updatePriceForm = new FormGroup({
    newPrice: new FormControl<number | null>(null, [Validators.required, Validators.min(1)]),
    reason: new FormControl('ปรับราคาขายตามคำแนะนำระบบ', [Validators.required])
  });

  // Change Status Form
  statusForm = new FormGroup({
    newStatus: new FormControl<string>('', [Validators.required]),
    reason: new FormControl(''),
    referenceNumber: new FormControl('')
  });

  readonly costTypeOptions = [
    { value: CostType.Repair.toString(), label: 'ค่าซ่อมแซม / อะไหล่ (Repair)' },
    { value: CostType.Cleaning.toString(), label: 'ค่าล้างทำความสะอาด / เซนเซอร์ (Cleaning)' },
    { value: CostType.Shipping.toString(), label: 'ค่าจัดส่ง / ขนส่ง (Shipping)' },
    { value: CostType.Other.toString(), label: 'ค่าใช้จ่ายอื่นๆ (Other)' },
    { value: CostType.Purchase.toString(), label: 'ราคารับซื้อสินค้า (Purchase)' }
  ];

  readonly statusOptions = [
    { value: InventoryStatus.Available.toString(), label: 'พร้อมขาย (Available in stock)' },
    { value: InventoryStatus.Reserved.toString(), label: 'จองแล้ว (Reserved)' },
    { value: InventoryStatus.Sold.toString(), label: 'ขายแล้ว (Sold)' },
    { value: InventoryStatus.Repair.toString(), label: 'ส่งซ่อม/บำรุงรักษา (Repair)' },
    { value: InventoryStatus.Returned.toString(), label: 'ลูกค้านำมาคืน (Returned)' },
    { value: InventoryStatus.WrittenOff.toString(), label: 'ตัดจำหน่าย (Written Off)' }
  ];

  readonly costSuggestions = [
    'ค่าล้างทำความสะอาดเซนเซอร์ (Sensor Cleaning)',
    'ค่าเปลี่ยนยางบวม / ยางกริป',
    'ค่าส่งพัสดุ / ขนส่งด่วน Kerry/Flash',
    'ค่าเปลี่ยนม่านชัตเตอร์แท้',
    'ค่าแบตเตอรี่เทียบเพิ่ม'
  ];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.itemId.set(id);
      this.loadAllData(id);
    }
  }

  loadAllData(id: string): void {
    this.inventoryService.getItemById(id).subscribe({
      next: item => {
        this.statusForm.controls.newStatus.setValue(item.status.toString());
        this.updatePriceForm.controls.newPrice.setValue(item.sellingPrice);
        this.imagesControl.setValue(item.imageUrls || []);
      }
    });

    this.inventoryService.getCostBreakdown(id).subscribe();
    this.inventoryService.getPricingRecommendation(id, this.targetMarginControl.value || 15).subscribe({
      next: rec => {
        this.targetMarginControl.setValue(rec.targetMarginPercentage, { emitEvent: false });
      }
    });
    this.inventoryService.getPriceHistories(id).subscribe();
    this.inventoryService.loadMovements(id).subscribe();
  }

  recalculatePricing(): void {
    const margin = this.targetMarginControl.value || 15;
    this.inventoryService.getPricingRecommendation(this.itemId(), margin).subscribe();
  }

  openAddCost(): void {
    this.costItemForm.reset({
      costType: CostType.Repair.toString(),
      amount: null,
      description: ''
    });
    this.showAddCostModal.set(true);
  }

  closeAddCost(): void {
    this.showAddCostModal.set(false);
  }

  setCostDescription(desc: string): void {
    this.costItemForm.controls.description.setValue(desc);
  }

  submitAddCostItem(): void {
    if (this.costItemForm.invalid) {
      this.costItemForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const val = this.costItemForm.getRawValue();

    this.inventoryService.addCostItem(this.itemId(), {
      costType: parseInt(val.costType!, 10) as CostType,
      amount: val.amount!,
      description: val.description || undefined
    }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.closeAddCost();
        this.feedbackMessage.set({ type: 'success', text: `บันทึกต้นทุนเพิ่มเติม +฿${val.amount} สำเร็จ` });
        this.recalculatePricing();
      },
      error: err => {
        this.isSubmitting.set(false);
        this.feedbackMessage.set({ type: 'error', text: err.error?.message ?? 'เกิดข้อผิดพลาดในการบันทึกต้นทุน' });
      }
    });
  }

  deleteCostItem(costId: string): void {
    if (!confirm('ยืนยันที่จะลบรายการต้นทุนนี้หรือไม่? ระบบจะปรับลดต้นทุนจริงให้อัตโนมัติ')) {
      return;
    }

    this.inventoryService.deleteCostItem(this.itemId(), costId).subscribe({
      next: () => {
        this.feedbackMessage.set({ type: 'success', text: 'ลบรายการต้นทุนสำเร็จ' });
        this.recalculatePricing();
      },
      error: err => {
        this.feedbackMessage.set({ type: 'error', text: err.error?.message ?? 'ไม่สามารถลบต้นทุนได้' });
      }
    });
  }

  openUpdatePrice(suggestedPrice?: number, reason?: string): void {
    const currentPrice = suggestedPrice ?? this.inventoryService.selectedItem()?.sellingPrice ?? null;
    this.updatePriceForm.setValue({
      newPrice: currentPrice,
      reason: reason ?? 'ปรับราคาขายตามคำแนะนำระบบ'
    });
    this.showUpdatePriceModal.set(true);
  }

  closeUpdatePrice(): void {
    this.showUpdatePriceModal.set(false);
  }

  submitUpdatePrice(): void {
    if (this.updatePriceForm.invalid) {
      this.updatePriceForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const val = this.updatePriceForm.getRawValue();

    this.inventoryService.updatePrice(this.itemId(), {
      newPrice: val.newPrice!,
      reason: val.reason || undefined
    }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.closeUpdatePrice();
        this.feedbackMessage.set({ type: 'success', text: `ปรับราคาขายเป็น ฿${val.newPrice} สำเร็จ` });
      },
      error: err => {
        this.isSubmitting.set(false);
        this.feedbackMessage.set({ type: 'error', text: err.error?.message ?? 'เกิดข้อผิดพลาดในการปรับราคา' });
      }
    });
  }

  openChangeStatus(): void {
    const current = this.inventoryService.selectedItem();
    if (current) {
      this.statusForm.controls.newStatus.setValue(current.status.toString());
    }
    this.showChangeStatusModal.set(true);
  }

  closeChangeStatus(): void {
    this.showChangeStatusModal.set(false);
  }

  submitChangeStatus(): void {
    if (this.statusForm.invalid) {
      this.statusForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const val = this.statusForm.getRawValue();

    this.inventoryService.updateStatus(this.itemId(), {
      newStatus: parseInt(val.newStatus!, 10) as InventoryStatus,
      reason: val.reason || undefined,
      referenceNumber: val.referenceNumber || undefined
    }).subscribe({
      next: updated => {
        this.isSubmitting.set(false);
        this.closeChangeStatus();
        this.feedbackMessage.set({ type: 'success', text: `อัปเดตสถานะเป็น '${updated.statusName}' สำเร็จ` });
      },
      error: err => {
        this.isSubmitting.set(false);
        this.feedbackMessage.set({ type: 'error', text: err.error?.message ?? 'เกิดข้อผิดพลาดในการเปลี่ยนสถานะ' });
      }
    });
  }

  // ==========================================
  // Image Gallery & Management
  // ==========================================

  openManageImages(): void {
    const item = this.inventoryService.selectedItem();
    this.imagesControl.setValue([...(item?.imageUrls || [])]);
    this.showManageImagesModal.set(true);
  }

  closeManageImages(): void {
    this.showManageImagesModal.set(false);
  }

  saveImages(): void {
    this.isSubmitting.set(true);
    const urls = this.imagesControl.value || [];
    this.inventoryService.updateImages(this.itemId(), urls).subscribe({
      next: updated => {
        this.isSubmitting.set(false);
        this.closeManageImages();
        this.feedbackMessage.set({
          type: 'success',
          text: `บันทึกรูปภาพสินค้า ${updated.sku} เรียบร้อยแล้ว (${updated.imageUrls?.length || 0} รูป)`
        });
      },
      error: err => {
        this.isSubmitting.set(false);
        this.feedbackMessage.set({
          type: 'error',
          text: err.error?.message ?? 'เกิดข้อผิดพลาดในการบันทึกรูปภาพ'
        });
      }
    });
  }

  getItemImages(item: InventoryItem | null): string[] {
    if (!item) return [];
    if (item.imageUrls && item.imageUrls.length > 0) {
      return item.imageUrls;
    }
    return item.productImageUrl ? [item.productImageUrl] : [];
  }

  selectImage(idx: number): void {
    this.activeImageIndex.set(idx);
  }

  prevImage(): void {
    const images = this.getItemImages(this.inventoryService.selectedItem());
    if (images.length <= 1) return;
    this.activeImageIndex.update(idx => (idx - 1 + images.length) % images.length);
  }

  nextImage(): void {
    const images = this.getItemImages(this.inventoryService.selectedItem());
    if (images.length <= 1) return;
    this.activeImageIndex.update(idx => (idx + 1) % images.length);
  }
}
