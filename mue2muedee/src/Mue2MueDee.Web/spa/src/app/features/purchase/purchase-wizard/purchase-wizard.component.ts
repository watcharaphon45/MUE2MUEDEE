import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize, of, switchMap, tap } from 'rxjs';
import { ActionDialogService } from '../../../shared/components/action-dialog/action-dialog.service';
import { PurchaseService } from '../purchase.service';
import { MasterDataService } from '../../master-data/master-data.service';
import {
  CategoryType,
  InspectionItemResult,
  InspectionTemplate,
  PurchaseOrder,
  PaymentMethod,
  MakeOfferRequest,
  SaveInspectionItemRequest
} from '../purchase.models';
import {
  TextboxComponent,
  NumberComponent,
  TextareaComponent,
  SelectComponent
} from '../../../shared/components';

import { NotificationService } from '../../../core/notifications/notification.service';

@Component({
  selector: 'app-purchase-wizard',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    TextboxComponent,
    NumberComponent,
    TextareaComponent,
    SelectComponent
  ],
  templateUrl: './purchase-wizard.component.html',
  styleUrl: './purchase-wizard.component.css'
})
export class PurchaseWizardComponent implements OnInit {
  private readonly notifications = inject(NotificationService);
  private readonly dialogs = inject(ActionDialogService);
  protected readonly purchaseService = inject(PurchaseService);
  protected readonly masterDataService = inject(MasterDataService);
  private readonly router = inject(Router);

  currentStep = signal<number>(1);
  isSubmitting = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  customerOptions = signal<{ value: string; label: string }[]>([]);
  productOptions = signal<{ value: string; label: string }[]>([]);
  gradeOptions = signal<{ value: string; label: string }[]>([]);

  template = signal<InspectionTemplate | null>(null);
  inspectionAnswers = signal<Map<string, { result: InspectionItemResult; notes: string }>>(new Map());

  createdPO = signal<PurchaseOrder | null>(null);
  readonly hasPendingSubmission = signal(false);
  private pendingOffer: MakeOfferRequest | null = null;
  private pendingInspection: SaveInspectionItemRequest[] = [];

  // Forms for each step
  step1Form = new FormGroup({
    customerId: new FormControl('', [Validators.required]),
    notes: new FormControl('')
  });

  step2Form = new FormGroup({
    productId: new FormControl('', [Validators.required]),
    serialNumber: new FormControl('', [Validators.required, Validators.pattern(/\S/)]),
    conditionGradeId: new FormControl('', [Validators.required]),
    shutterCount: new FormControl<number | null>(null, [Validators.min(0)]),
    accessories: new FormControl(''),
    defects: new FormControl('')
  });

  step3Form = new FormGroup({
    expectedSalePrice: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]),
    estimatedRepairCost: new FormControl<number>(0, [Validators.min(0)]),
    estimatedCleaningCost: new FormControl<number>(0, [Validators.min(0)]),
    missingAccessoriesDeduction: new FormControl<number>(0, [Validators.min(0)]),
    offeredPrice: new FormControl<number | null>(null, [Validators.required, Validators.min(0.01)]),
    offerNotes: new FormControl('')
  });

  paymentForm = new FormGroup({
    paymentMethod: new FormControl<string>(PaymentMethod.BankTransfer.toString(), [Validators.required]),
    amount: new FormControl<number | null>(null, [Validators.required, Validators.min(0.01)]),
    referenceNumber: new FormControl(''),
    notes: new FormControl('')
  });

  readonly paymentMethodOptions = [
    { value: PaymentMethod.BankTransfer.toString(), label: 'โอนผ่านบัญชีธนาคาร (Bank Transfer)' },
    { value: PaymentMethod.PromptPay.toString(), label: 'พร้อมเพย์ (PromptPay)' },
    { value: PaymentMethod.Cash.toString(), label: 'เงินสดหน้าร้าน (Cash)' }
  ];

  ngOnInit(): void {
    this.loadDropdowns();

    // Auto-calculate Max Buy Price whenever financial inputs change in Step 3
    this.step3Form.valueChanges.subscribe(() => {
      const maxBuy = this.calculatedMaxBuy;
      if (!this.step3Form.controls.offeredPrice.dirty || !this.step3Form.controls.offeredPrice.value) {
        this.step3Form.controls.offeredPrice.setValue(maxBuy, { emitEvent: false });
      }
    });

    // When product selected, load inspection checklist
    this.step2Form.controls.productId.valueChanges.subscribe(prodId => {
      if (prodId) {
        const prod = this.masterDataService.products().find(p => p.id === prodId);
        if (prod) {
          // Default selling price
          if (prod.defaultSellingPrice) {
            this.step3Form.controls.expectedSalePrice.setValue(prod.defaultSellingPrice);
          }

          // Fetch inspection checklist (Camera or Lens)
          const cat = this.masterDataService.categories().find(c => c.id === prod.categoryId);
          const type = (cat?.type as CategoryType) || CategoryType.Camera;
          this.purchaseService.loadInspectionTemplate(type).subscribe(tpl => {
            this.template.set(tpl);
            // Initialize answers as Pass by default
            const initialMap = new Map<string, { result: InspectionItemResult; notes: string }>();
            tpl.items.forEach(item => {
              initialMap.set(item.id, { result: InspectionItemResult.Pass, notes: '' });
            });
            this.inspectionAnswers.set(initialMap);
          });
        }
      }
    });
  }

  private loadDropdowns(): void {
    this.masterDataService.loadCustomers().subscribe(customers => {
      this.customerOptions.set(
        customers.map(c => ({
          value: c.id,
          label: `${c.name} (${c.phone}) ${c.idCardNumber ? '• ปชช: ' + c.idCardNumber : ''}`
        }))
      );
    });

    this.masterDataService.loadProducts().subscribe(products => {
      this.productOptions.set(
        products.map(p => ({
          value: p.id,
          label: `${p.brandName} ${p.modelName} (${p.productCode})`
        }))
      );
    });

    this.masterDataService.loadConditionGrades().subscribe(grades => {
      this.gradeOptions.set(
        grades.map(g => ({
          value: g.id,
          label: `เกรด ${g.gradeCode} — ${g.gradeName}`
        }))
      );
    });

    this.masterDataService.loadCategories().subscribe();
  }

  // --- Real-time Calculations ---

  get calculatedMaxBuy(): number {
    const sale = this.step3Form.value.expectedSalePrice || 0;
    const repair = this.step3Form.value.estimatedRepairCost || 0;
    const clean = this.step3Form.value.estimatedCleaningCost || 0;
    const missing = this.step3Form.value.missingAccessoriesDeduction || 0;
    return PurchaseService.calculateMaxBuyPrice(sale, repair, clean, missing, 0.15);
  }

  get isOverMaxBuy(): boolean {
    const offer = this.step3Form.value.offeredPrice || 0;
    return offer > this.calculatedMaxBuy;
  }

  // --- Checklist Answer Handling ---

  setResult(itemId: string, result: InspectionItemResult): void {
    const current = new Map(this.inspectionAnswers());
    const existing = current.get(itemId) || { result: InspectionItemResult.Pass, notes: '' };
    current.set(itemId, { ...existing, result });
    this.inspectionAnswers.set(current);
  }

  setNotes(itemId: string, notes: string): void {
    const current = new Map(this.inspectionAnswers());
    const existing = current.get(itemId) || { result: InspectionItemResult.Pass, notes: '' };
    current.set(itemId, { ...existing, notes });
    this.inspectionAnswers.set(current);
  }

  getAnswer(itemId: string): { result: InspectionItemResult; notes: string } {
    return this.inspectionAnswers().get(itemId) || { result: InspectionItemResult.Pass, notes: '' };
  }

  // --- Wizard Navigation ---

  goToStep(step: number): void {
    if (this.isSubmitting() || this.createdPO()) return;
    this.errorMessage.set(null);

    if (step >= 2 && this.step1Form.invalid) {
      this.notifications.validation();
      this.step1Form.markAllAsTouched();
      return;
    }

    if (step >= 3 && this.step2Form.invalid) {
      this.notifications.validation();
      this.step2Form.markAllAsTouched();
      return;
    }

    this.currentStep.set(step);
  }

  // --- Submit Final PO in Step 3 ---

  createAndSubmitPO(): void {
    if (this.isSubmitting() || this.createdPO()) return;
    if (this.step1Form.invalid || this.step2Form.invalid || this.step3Form.invalid) {
      this.notifications.validation();
      this.step1Form.markAllAsTouched();
      this.step2Form.markAllAsTouched();
      this.step3Form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const s1 = this.step1Form.getRawValue();
    const s2 = this.step2Form.getRawValue();
    const s3 = this.step3Form.getRawValue();
    this.pendingOffer = { totalOfferedPrice: s3.offeredPrice!, notes: s3.offerNotes || undefined };
    this.pendingInspection = Array.from(this.inspectionAnswers().entries()).map(([templateItemId, answer]) => ({
      templateItemId, result: answer.result, notes: answer.notes || undefined
    }));

    // 1. Create Purchase Order with item
    this.purchaseService.createOrder({
      customerId: s1.customerId!,
      notes: s1.notes || undefined,
      initialItems: [{
        productId: s2.productId!,
        serialNumber: s2.serialNumber!.trim(),
        conditionGradeId: s2.conditionGradeId!,
        shutterCount: s2.shutterCount ?? undefined,
        accessories: s2.accessories || undefined,
        defects: s2.defects || undefined,
        expectedSalePrice: s3.expectedSalePrice!,
        estimatedRepairCost: s3.estimatedRepairCost || 0,
        estimatedCleaningCost: s3.estimatedCleaningCost || 0,
        missingAccessoriesDeduction: s3.missingAccessoriesDeduction || 0,
        offeredPrice: s3.offeredPrice!
      }]
    }).subscribe({
      next: po => {
        this.createdPO.set(po);
        this.hasPendingSubmission.set(true);
        this.currentStep.set(4);
        this.isSubmitting.set(false);
        this.completeSubmission();
      },
      error: err => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.message ?? 'เกิดข้อผิดพลาดในการสร้างใบรับซื้อ');
      }
    });
  }

  completeSubmission(): void {
    const po = this.createdPO();
    const offer = this.pendingOffer;
    if (this.isSubmitting() || !po || !offer || !this.hasPendingSubmission()) return;
    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    const inspection = this.pendingInspection.length > 0 && po.items[0]
      ? this.purchaseService.saveInspection(po.id, po.items[0].id, this.pendingInspection)
      : of(po);
    inspection.pipe(
      tap(updated => this.createdPO.set(updated)),
      switchMap(() => this.purchaseService.makeOffer(po.id, offer)),
      finalize(() => this.isSubmitting.set(false))
    ).subscribe({
      next: updated => {
        this.createdPO.set(updated);
        this.hasPendingSubmission.set(false);
        this.paymentForm.controls.amount.setValue(updated.totalOfferedPrice);
      },
      error: err => this.errorMessage.set(err.error?.message ?? 'บันทึกผลตรวจหรือเสนอราคาไม่สำเร็จ กรุณาลองใหม่กับใบรับซื้อเดิม')
    });
  }

  // --- Step 4 Actions ---

  async approveOverMax(): Promise<void> {
    const po = this.createdPO();
    if (!po) return;
    if (!await this.confirmAction('อนุมัติราคาพิเศษ', `ยืนยันอนุมัติราคาเสนอซื้อเกินเกณฑ์ของใบรับซื้อ ${po.poNumber} หรือไม่?`)) return;

    this.isSubmitting.set(true);
    this.purchaseService.approvePurchase(po.id).subscribe({
      next: updated => {
        this.createdPO.set(updated);
        this.isSubmitting.set(false);
      },
      error: err => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.message ?? 'ไม่สามารถอนุมัติได้');
      }
    });
  }

  async acceptOffer(): Promise<void> {
    const po = this.createdPO();
    if (!po) return;
    if (!await this.confirmAction('ลูกค้าตกลงรับข้อเสนอ', `ยืนยันว่าลูกค้าตกลงขายสินค้าตามข้อเสนอในใบรับซื้อ ${po.poNumber} หรือไม่?`)) return;

    this.isSubmitting.set(true);
    this.purchaseService.acceptOffer(po.id).subscribe({
      next: updated => {
        this.createdPO.set(updated);
        this.paymentForm.controls.amount.setValue(updated.totalAgreedPrice);
        this.isSubmitting.set(false);
      },
      error: err => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.message ?? 'เกิดข้อผิดพลาดในการตอบรับข้อเสนอ');
      }
    });
  }

  async rejectOffer(): Promise<void> {
    const po = this.createdPO();
    if (!po) return;
    if (!await this.confirmAction('ปฏิเสธข้อเสนอรับซื้อ', `ยืนยันยกเลิก/ปฏิเสธข้อเสนอของใบรับซื้อ ${po.poNumber} หรือไม่?`, true)) return;

    this.isSubmitting.set(true);
    this.purchaseService.rejectOffer(po.id).subscribe({
      next: updated => {
        this.createdPO.set(updated);
        this.isSubmitting.set(false);
      },
      error: err => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.message ?? 'เกิดข้อผิดพลาดในการยกเลิกข้อเสนอ');
      }
    });
  }

  submitPayment(): void {
    if (this.isSubmitting() || this.hasPendingSubmission()) return;
    const po = this.createdPO();
    if (!po || this.paymentForm.invalid) {
      this.notifications.validation();
      this.paymentForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const val = this.paymentForm.getRawValue();

    this.purchaseService.recordPayment(po.id, {
      paymentMethod: parseInt(val.paymentMethod!, 10) as PaymentMethod,
      amount: val.amount!,
      referenceNumber: val.referenceNumber || undefined,
      notes: val.notes || undefined
    }).subscribe({
      next: updated => {
        this.createdPO.set(updated);
        this.paymentForm.controls.amount.setValue(Math.max(0,
          Math.round((updated.totalAgreedPrice - updated.payments.reduce((sum, p) => sum + p.amount, 0)) * 100) / 100));
        this.isSubmitting.set(false);
      },
      error: err => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.message ?? 'เกิดข้อผิดพลาดในการบันทึกการจ่ายเงิน');
      }
    });
  }

  async receiveToStock(): Promise<void> {
    const po = this.createdPO();
    if (!po) return;
    if (!await this.confirmAction('รับสินค้าเข้าคลัง', `ยืนยันรับสินค้าจากใบรับซื้อ ${po.poNumber} เข้าคลัง? ระบบจะสร้าง SKU และบันทึกสต็อกให้อัตโนมัติ`)) return;

    this.isSubmitting.set(true);
    this.purchaseService.receiveToStock(po.id).subscribe({
      next: updated => {
        this.createdPO.set(updated);
        this.isSubmitting.set(false);
        this.router.navigate(['/inventory']);
      },
      error: err => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.message ?? 'เกิดข้อผิดพลาดในการนำเข้าสต็อก');
      }
    });
  }
  private async confirmAction(title: string, message: string, danger = false): Promise<boolean> {
    if (this.isSubmitting() || this.hasPendingSubmission()) return false;
    this.isSubmitting.set(true);
    const accepted = await this.dialogs.confirm({ title, message, tone: danger ? 'danger' : 'primary' });
    if (!accepted) this.isSubmitting.set(false);
    return accepted;
  }
}
