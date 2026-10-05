import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MasterDataService } from '../master-data.service';
import { Product } from '../master-data.models';
import {
  TextboxComponent,
  NumberComponent,
  TextareaComponent,
  SelectComponent
} from '../../../shared/components';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TextboxComponent,
    NumberComponent,
    TextareaComponent,
    SelectComponent
  ],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.css'
})
export class ProductListComponent implements OnInit {
  protected readonly masterDataService = inject(MasterDataService);

  showCreateModal = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);
  feedback = signal<{ type: 'success' | 'error'; text: string } | null>(null);

  brandOptions = signal<{ value: string; label: string }[]>([]);
  categoryOptions = signal<{ value: string; label: string }[]>([]);

  searchControl = new FormControl('');

  form = new FormGroup({
    brandId: new FormControl('', [Validators.required]),
    categoryId: new FormControl('', [Validators.required]),
    modelName: new FormControl('', [Validators.required]),
    productCode: new FormControl('', [Validators.required]),
    description: new FormControl(''),
    defaultSellingPrice: new FormControl<number | null>(null)
  });

  ngOnInit(): void {
    this.loadData();

    this.searchControl.valueChanges.subscribe(val => {
      this.masterDataService.loadProducts(undefined, undefined, val || undefined).subscribe();
    });
  }

  loadData(): void {
    this.masterDataService.loadBrands().subscribe(brands => {
      this.brandOptions.set(brands.map(b => ({ value: b.id, label: b.name })));
    });

    this.masterDataService.loadCategories().subscribe(categories => {
      this.categoryOptions.set(categories.map(c => ({ value: c.id, label: `${c.name} (${c.typeName})` })));
    });

    this.masterDataService.loadProducts().subscribe();
  }

  openCreate(): void {
    this.form.reset();
    this.showCreateModal.set(true);
  }

  closeCreate(): void {
    this.showCreateModal.set(false);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const val = this.form.getRawValue();

    this.masterDataService.createProduct({
      brandId: val.brandId!,
      categoryId: val.categoryId!,
      modelName: val.modelName!.trim(),
      productCode: val.productCode!.trim().toUpperCase(),
      description: val.description || undefined,
      defaultSellingPrice: val.defaultSellingPrice || undefined
    }).subscribe({
      next: created => {
        this.isSubmitting.set(false);
        this.closeCreate();
        this.feedback.set({ type: 'success', text: `เพิ่มรุ่นสินค้า '${created.modelName}' สำเร็จ` });
      },
      error: err => {
        this.isSubmitting.set(false);
        this.feedback.set({ type: 'error', text: err.error?.message ?? 'เกิดข้อผิดพลาดในการสร้างสินค้า' });
      }
    });
  }
}
