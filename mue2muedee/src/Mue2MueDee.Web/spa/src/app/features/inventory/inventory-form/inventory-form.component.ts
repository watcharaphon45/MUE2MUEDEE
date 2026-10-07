import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryService } from '../inventory.service';
import { MasterDataService } from '../../master-data/master-data.service';
import {
  TextboxComponent,
  NumberComponent,
  TextareaComponent,
  SelectComponent
} from '../../../shared/components';

@Component({
  selector: 'app-inventory-form',
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
  templateUrl: './inventory-form.component.html',
  styleUrl: './inventory-form.component.css'
})
export class InventoryFormComponent implements OnInit {
  private readonly inventoryService = inject(InventoryService);
  private readonly masterDataService = inject(MasterDataService);
  private readonly router = inject(Router);

  productOptions = signal<{ value: string; label: string }[]>([]);
  gradeOptions = signal<{ value: string; label: string }[]>([]);
  customerOptions = signal<{ value: string; label: string }[]>([]);

  selectedProduct = signal<any | null>(null);

  isSubmitting = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  form = new FormGroup({
    productId: new FormControl('', [Validators.required]),
    serialNumber: new FormControl('', [Validators.required]),
    conditionGradeId: new FormControl('', [Validators.required]),
    purchaseCost: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]),
    actualCost: new FormControl<number | null>(null),
    sellingPrice: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]),
    shutterCount: new FormControl<number | null>(null),
    accessories: new FormControl(''),
    defects: new FormControl(''),
    customerId: new FormControl(''),
    notes: new FormControl('')
  });

  ngOnInit(): void {
    this.loadDropdowns();

    // Auto calculate actual cost default when purchase cost changes if actual cost is blank
    this.form.controls.purchaseCost.valueChanges.subscribe(val => {
      if (val && !this.form.controls.actualCost.value) {
        this.form.controls.actualCost.setValue(val);
      }
    });

    // Auto set default selling price if selected product has one
    this.form.controls.productId.valueChanges.subscribe(productId => {
      if (productId) {
        const prod = this.masterDataService.products().find(p => p.id === productId);
        this.selectedProduct.set(prod || null);
        if (prod && prod.defaultSellingPrice && !this.form.controls.sellingPrice.value) {
          this.form.controls.sellingPrice.setValue(prod.defaultSellingPrice);
        }
      } else {
        this.selectedProduct.set(null);
      }
    });
  }

  private loadDropdowns(): void {
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

    this.masterDataService.loadCustomers().subscribe(customers => {
      this.customerOptions.set([
        { value: '', label: '-- ไม่ระบุลูกค้า / รับซื้อหน้าร้านนิรนาม --' },
        ...customers.map(c => ({
          value: c.id,
          label: `${c.name} (${c.phone})`
        }))
      ]);
    });
  }

  // Calculated Real-Time Metrics
  get expectedProfit(): number {
    const cost = this.form.value.actualCost || this.form.value.purchaseCost || 0;
    const price = this.form.value.sellingPrice || 0;
    return price - cost;
  }

  get expectedMargin(): number {
    const cost = this.form.value.actualCost || this.form.value.purchaseCost || 0;
    if (cost <= 0) return 0;
    return Math.round((this.expectedProfit / cost) * 100);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const val = this.form.getRawValue();

    this.inventoryService.createItem({
      productId: val.productId!,
      serialNumber: val.serialNumber!,
      conditionGradeId: val.conditionGradeId!,
      purchaseCost: val.purchaseCost!,
      actualCost: val.actualCost || val.purchaseCost!,
      sellingPrice: val.sellingPrice!,
      shutterCount: val.shutterCount || undefined,
      accessories: val.accessories || undefined,
      defects: val.defects || undefined,
      notes: val.notes || undefined,
      customerId: val.customerId || undefined
    }).subscribe({
      next: created => {
        this.isSubmitting.set(false);
        this.router.navigate(['/inventory', created.id]);
      },
      error: err => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.message ?? 'เกิดข้อผิดพลาดในการบันทึกข้อมูลสินค้า');
      }
    });
  }
}
