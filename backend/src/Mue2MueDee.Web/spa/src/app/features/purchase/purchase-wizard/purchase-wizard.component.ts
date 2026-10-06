import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { PurchaseService } from '../purchase.service';
import { MasterDataService } from '../../master-data/master-data.service';
import {
  CategoryType,
  InspectionItemResult,
  InspectionTemplate,
  PurchaseOrder,
  PaymentMethod
} from '../purchase.models';
import {
  TextboxComponent,
  NumberComponent,
  TextareaComponent,
  SelectComponent
} from '../../../shared/components';

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

  // Forms for each step
  step1Form = new FormGroup({
    customerId: new FormControl('', [Validators.required]),
    notes: new FormControl('')
  });

  step2Form = new FormGroup({
    productId: new FormControl('', [Validators.required]),
    serialNumber: new FormControl('', [Validators.required]),
    conditionGradeId: new FormControl('', [Validators.required]),
    shutterCount: new FormControl<number | null>(null),
    accessories: new FormControl(''),
    defects: new FormControl('')
  });

  step3Form = new FormGroup({
    expectedSalePrice: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]),
    estimatedRepairCost: new FormControl<number>(0),
    estimatedCleaningCost: new FormControl<number>(0),
    missingAccessoriesDeduction: new FormControl<number>(0),
    offeredPrice: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]),
    offerNotes: new FormControl('')
  });

  paymentForm = new FormGroup({
    paymentMethod: new FormControl<string>(PaymentMethod.BankTransfer.toString(), [Validators.required]),
    amount: new FormControl<number | null>(null, [Validators.required, Validators.min(1)]),
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
    this.errorMessage.set(null);

    if (step === 2 && this.step1Form.invalid) {
      this.step1Form.markAllAsTouched();
      return;
    }

    if (step === 3 && this.step2Form.invalid) {
      this.step2Form.markAllAsTouched();
      return;
    }

    this.currentStep.set(step);
  }

  // --- Submit Final PO in Step 3 ---

  createAndSubmitPO(): void {
    if (this.step3Form.invalid) {
      this.step3Form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const s1 = this.step1Form.getRawValue();
    const s2 = this.step2Form.getRawValue();
    const s3 = this.step3Form.getRawValue();

    // 1. Create Purchase Order with item
    this.purchaseService.createOrder({
      customerId: s1.customerId!,
      notes: s1.notes || undefined,
      initialItems: [{
        productId: s2.productId!,
        serialNumber: s2.serialNumber!.trim(),
        conditionGradeId: s2.conditionGradeId!,
        shutterCount: s2.shutterCount || undefined,
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
        const itemId = po.items[0]?.id;

        // 2. Save Inspection Results
        if (itemId && this.inspectionAnswers().size > 0) {
          const resultsList = Array.from(this.inspectionAnswers().entries()).map(([tid, ans]) => ({
            templateItemId: tid,
            result: ans.result,
            notes: ans.notes || undefined
          }));

          this.purchaseService.saveInspection(po.id, itemId, resultsList).subscribe();
        }

        // 3. Make offer to set offered status
        this.purchaseService.makeOffer(po.id, {
          totalOfferedPrice: s3.offeredPrice!,
          notes: s3.offerNotes || undefined
        }).subscribe({
          next: updatedPo => {
            this.createdPO.set(updatedPo);
            this.paymentForm.controls.amount.setValue(updatedPo.totalOfferedPrice);
            this.isSubmitting.set(false);
            this.currentStep.set(4); // Advance to finalize step
          },
          error: () => {
            this.isSubmitting.set(false);
            this.currentStep.set(4);
          }
        });
      },
      error: err => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.message ?? 'เกิดข้อผิดพลาดในการสร้างใบรับซื้อ');
      }
    });
  }

  // --- Step 4 Actions ---

  approveOverMax(): void {
    const po = this.createdPO();
    if (!po) return;

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

  acceptOffer(): void {
    const po = this.createdPO();
    if (!po) return;

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

  rejectOffer(): void {
    const po = this.createdPO();
    if (!po) return;

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
    const po = this.createdPO();
    if (!po || this.paymentForm.invalid) {
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
        this.isSubmitting.set(false);
      },
      error: err => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.message ?? 'เกิดข้อผิดพลาดในการบันทึกการจ่ายเงิน');
      }
    });
  }

  receiveToStock(): void {
    const po = this.createdPO();
    if (!po) return;

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
}
