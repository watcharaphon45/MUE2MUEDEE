import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MasterDataService } from '../master-data.service';
import {
  Brand,
  Category,
  ConditionGrade,
  Customer,
  Product
} from '../master-data.models';
import {
  TextboxComponent,
  NumberComponent,
  TextareaComponent,
  SelectComponent,
  CheckboxComponent,
  SelectOption,
  ImageUploaderComponent
} from '../../../shared/components';

export type MasterTab = 'products' | 'brands' | 'categories' | 'grades' | 'customers';

@Component({
  selector: 'app-master-data-hub',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TextboxComponent,
    NumberComponent,
    TextareaComponent,
    SelectComponent,
    CheckboxComponent,
    ImageUploaderComponent
  ],
  templateUrl: './master-data-hub.component.html',
  styleUrl: './master-data-hub.component.css'
})
export class MasterDataHubComponent implements OnInit {
  protected readonly masterDataService = inject(MasterDataService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  activeTab = signal<MasterTab>('products');
  searchTerm = new FormControl('');

  // Modals & Submissions
  showModal = signal<boolean>(false);
  modalMode = signal<'create' | 'edit'>('create');
  currentEditId = signal<string | null>(null);

  showDeleteModal = signal<boolean>(false);
  itemToDelete = signal<{ type: MasterTab; id: string; name: string } | null>(null);

  isSubmitting = signal<boolean>(false);
  feedback = signal<{ type: 'success' | 'error'; text: string } | null>(null);

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

  // Filters for Products Tab
  productBrandFilter = new FormControl('');
  productCategoryFilter = new FormControl('');

  // Category Types Options
  readonly categoryTypeOptions: SelectOption[] = [
    { value: 1, label: 'กล้องถ่ายภาพ (Camera)' },
    { value: 2, label: 'เลนส์กล้อง (Lens)' },
    { value: 3, label: 'อุปกรณ์เสริม & แฟลช (Accessory)' }
  ];

  // Customer Types Options
  readonly customerTypeOptions: SelectOption[] = [
    { value: 1, label: 'บุคคลธรรมดา (Individual Seller / Buyer)' },
    { value: 2, label: 'ร้านค้า / นิติบุคคล (Business / Shop Partner)' }
  ];

  // Reactive Forms for Each Master Entity
  brandForm = new FormGroup({
    name: new FormControl('', [Validators.required]),
    description: new FormControl(''),
    logoUrl: new FormControl(''),
    isActive: new FormControl(true)
  });

  categoryForm = new FormGroup({
    name: new FormControl('', [Validators.required]),
    type: new FormControl<number>(1, [Validators.required]),
    description: new FormControl(''),
    isActive: new FormControl(true)
  });

  gradeForm = new FormGroup({
    gradeCode: new FormControl('', [Validators.required]),
    gradeName: new FormControl('', [Validators.required]),
    description: new FormControl(''),
    sortOrder: new FormControl<number>(1, [Validators.required, Validators.min(1)]),
    isActive: new FormControl(true)
  });

  productForm = new FormGroup({
    brandId: new FormControl('', [Validators.required]),
    categoryId: new FormControl('', [Validators.required]),
    modelName: new FormControl('', [Validators.required]),
    productCode: new FormControl('', [Validators.required]),
    description: new FormControl(''),
    defaultSellingPrice: new FormControl<number | null>(null),
    imageUrl: new FormControl(''),
    isActive: new FormControl(true)
  });

  customerForm = new FormGroup({
    customerType: new FormControl<number>(1, [Validators.required]),
    name: new FormControl('', [Validators.required]),
    phone: new FormControl('', [Validators.required]),
    email: new FormControl(''),
    idCardNumber: new FormControl(''),
    address: new FormControl(''),
    lineId: new FormControl(''),
    facebook: new FormControl(''),
    bankName: new FormControl(''),
    bankAccountNumber: new FormControl(''),
    bankAccountName: new FormControl(''),
    notes: new FormControl(''),
    isActive: new FormControl(true)
  });

  // Options for Dropdowns in Product Form
  brandSelectOptions = computed<SelectOption[]>(() =>
    this.masterDataService.brands().map(b => ({ value: b.id, label: b.name }))
  );

  categorySelectOptions = computed<SelectOption[]>(() =>
    this.masterDataService.categories().map(c => ({ value: c.id, label: `${c.name} (${c.typeName})` }))
  );

  // Filtered Lists for Display
  filteredBrands = computed(() => {
    const q = (this.searchTerm.value || '').trim().toLowerCase();
    const list = this.masterDataService.brands();
    if (!q) return list;
    return list.filter(b => b.name.toLowerCase().includes(q) || (b.description && b.description.toLowerCase().includes(q)));
  });

  filteredCategories = computed(() => {
    const q = (this.searchTerm.value || '').trim().toLowerCase();
    const list = this.masterDataService.categories();
    if (!q) return list;
    return list.filter(c => c.name.toLowerCase().includes(q) || c.typeName.toLowerCase().includes(q));
  });

  filteredGrades = computed(() => {
    const q = (this.searchTerm.value || '').trim().toLowerCase();
    const list = this.masterDataService.conditionGrades();
    if (!q) return list;
    return list.filter(g => g.gradeCode.toLowerCase().includes(q) || g.gradeName.toLowerCase().includes(q));
  });

  filteredProducts = computed(() => {
    const q = (this.searchTerm.value || '').trim().toLowerCase();
    const brandId = this.productBrandFilter.value;
    const catId = this.productCategoryFilter.value;
    let list = this.masterDataService.products();

    if (brandId) {
      list = list.filter(p => p.brandId === brandId);
    }
    if (catId) {
      list = list.filter(p => p.categoryId === catId);
    }
    if (q) {
      list = list.filter(p =>
        p.modelName.toLowerCase().includes(q) ||
        p.productCode.toLowerCase().includes(q) ||
        p.brandName.toLowerCase().includes(q) ||
        p.categoryName.toLowerCase().includes(q)
      );
    }
    return list;
  });

  filteredCustomers = computed(() => {
    const q = (this.searchTerm.value || '').trim().toLowerCase();
    const list = this.masterDataService.customers();
    if (!q) return list;
    return list.filter(c =>
      c.name.toLowerCase().includes(q) ||
      c.phone.toLowerCase().includes(q) ||
      (c.idCardNumber && c.idCardNumber.includes(q))
    );
  });

  ngOnInit(): void {
    // Check tab query parameter
    this.route.queryParams.subscribe(params => {
      if (params['tab'] && ['products', 'brands', 'categories', 'grades', 'customers'].includes(params['tab'])) {
        this.activeTab.set(params['tab'] as MasterTab);
      }
    });

    this.loadAllMasterData();
  }

  setTab(tab: MasterTab): void {
    this.activeTab.set(tab);
    this.searchTerm.setValue('');
    this.router.navigate([], { relativeTo: this.route, queryParams: { tab } });
  }

  loadAllMasterData(): void {
    this.masterDataService.loadBrands().subscribe();
    this.masterDataService.loadCategories().subscribe();
    this.masterDataService.loadConditionGrades().subscribe();
    this.masterDataService.loadProducts().subscribe();
    this.masterDataService.loadCustomers().subscribe();
  }

  // ==========================================
  // CREATE / EDIT MODAL OPENERS
  // ==========================================

  openCreateModal(): void {
    this.modalMode.set('create');
    this.currentEditId.set(null);

    switch (this.activeTab()) {
      case 'brands':
        this.brandForm.reset({ name: '', description: '', logoUrl: '', isActive: true });
        break;
      case 'categories':
        this.categoryForm.reset({ name: '', type: 1, description: '', isActive: true });
        break;
      case 'grades':
        this.gradeForm.reset({ gradeCode: '', gradeName: '', description: '', sortOrder: (this.masterDataService.conditionGrades().length + 1), isActive: true });
        break;
      case 'products':
        this.productForm.reset({ brandId: '', categoryId: '', modelName: '', productCode: '', description: '', defaultSellingPrice: null, imageUrl: '', isActive: true });
        break;
      case 'customers':
        this.customerForm.reset({ customerType: 1, name: '', phone: '', email: '', idCardNumber: '', address: '', lineId: '', facebook: '', bankName: '', bankAccountNumber: '', bankAccountName: '', notes: '', isActive: true });
        break;
    }

    this.showModal.set(true);
  }

  openEditModal(item: any): void {
    this.modalMode.set('edit');
    this.currentEditId.set(item.id);

    switch (this.activeTab()) {
      case 'brands':
        this.brandForm.patchValue({
          name: item.name,
          description: item.description || '',
          logoUrl: item.logoUrl || '',
          isActive: item.isActive
        });
        break;
      case 'categories':
        this.categoryForm.patchValue({
          name: item.name,
          type: item.type,
          description: item.description || '',
          isActive: item.isActive
        });
        break;
      case 'grades':
        this.gradeForm.patchValue({
          gradeCode: item.gradeCode,
          gradeName: item.gradeName,
          description: item.description || '',
          sortOrder: item.sortOrder,
          isActive: item.isActive
        });
        break;
      case 'products':
        this.productForm.patchValue({
          brandId: item.brandId,
          categoryId: item.categoryId,
          modelName: item.modelName,
          productCode: item.productCode,
          description: item.description || '',
          defaultSellingPrice: item.defaultSellingPrice ?? null,
          imageUrl: item.imageUrl || '',
          isActive: item.isActive
        });
        break;
      case 'customers':
        this.customerForm.patchValue({
          customerType: item.customerType,
          name: item.name,
          phone: item.phone,
          email: item.email || '',
          idCardNumber: item.idCardNumber || '',
          address: item.address || '',
          lineId: item.lineId || '',
          facebook: item.facebook || '',
          bankName: item.bankName || '',
          bankAccountNumber: item.bankAccountNumber || '',
          bankAccountName: item.bankAccountName || '',
          notes: item.notes || '',
          isActive: item.isActive
        });
        break;
    }

    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
  }

  // ==========================================
  // FORM SUBMISSION (CREATE & UPDATE)
  // ==========================================

  submitForm(): void {
    const tab = this.activeTab();
    const mode = this.modalMode();
    const editId = this.currentEditId();

    if (tab === 'brands') {
      if (this.brandForm.invalid) {
        this.brandForm.markAllAsTouched();
        return;
      }
      this.isSubmitting.set(true);
      const val = this.brandForm.getRawValue();

      if (mode === 'create') {
        this.masterDataService.createBrand({
          name: val.name!.trim(),
          description: val.description || undefined,
          logoUrl: val.logoUrl || undefined
        }).subscribe({
          next: () => this.handleSuccess('เพิ่มแบรนด์สำเร็จ'),
          error: err => this.handleError(err)
        });
      } else if (editId) {
        this.masterDataService.updateBrand(editId, {
          name: val.name!.trim(),
          description: val.description || undefined,
          logoUrl: val.logoUrl || undefined,
          isActive: val.isActive ?? true
        }).subscribe({
          next: () => this.handleSuccess('แก้ไขแบรนด์สำเร็จ'),
          error: err => this.handleError(err)
        });
      }
    } else if (tab === 'categories') {
      if (this.categoryForm.invalid) {
        this.categoryForm.markAllAsTouched();
        return;
      }
      this.isSubmitting.set(true);
      const val = this.categoryForm.getRawValue();

      if (mode === 'create') {
        this.masterDataService.createCategory({
          name: val.name!.trim(),
          type: Number(val.type),
          description: val.description || undefined
        }).subscribe({
          next: () => this.handleSuccess('เพิ่มหมวดหมู่สำเร็จ'),
          error: err => this.handleError(err)
        });
      } else if (editId) {
        this.masterDataService.updateCategory(editId, {
          name: val.name!.trim(),
          type: Number(val.type),
          description: val.description || undefined,
          isActive: val.isActive ?? true
        }).subscribe({
          next: () => this.handleSuccess('แก้ไขหมวดหมู่สำเร็จ'),
          error: err => this.handleError(err)
        });
      }
    } else if (tab === 'grades') {
      if (this.gradeForm.invalid) {
        this.gradeForm.markAllAsTouched();
        return;
      }
      this.isSubmitting.set(true);
      const val = this.gradeForm.getRawValue();

      if (mode === 'create') {
        this.masterDataService.createConditionGrade({
          gradeCode: val.gradeCode!.trim().toUpperCase(),
          gradeName: val.gradeName!.trim(),
          description: val.description || undefined,
          sortOrder: Number(val.sortOrder)
        }).subscribe({
          next: () => this.handleSuccess('เพิ่มเกรดสภาพสำเร็จ'),
          error: err => this.handleError(err)
        });
      } else if (editId) {
        this.masterDataService.updateConditionGrade(editId, {
          gradeCode: val.gradeCode!.trim().toUpperCase(),
          gradeName: val.gradeName!.trim(),
          description: val.description || undefined,
          sortOrder: Number(val.sortOrder),
          isActive: val.isActive ?? true
        }).subscribe({
          next: () => this.handleSuccess('แก้ไขเกรดสภาพสำเร็จ'),
          error: err => this.handleError(err)
        });
      }
    } else if (tab === 'products') {
      if (this.productForm.invalid) {
        this.productForm.markAllAsTouched();
        return;
      }
      this.isSubmitting.set(true);
      const val = this.productForm.getRawValue();

      if (mode === 'create') {
        this.masterDataService.createProduct({
          brandId: val.brandId!,
          categoryId: val.categoryId!,
          modelName: val.modelName!.trim(),
          productCode: val.productCode!.trim().toUpperCase(),
          description: val.description || undefined,
          defaultSellingPrice: val.defaultSellingPrice || undefined,
          imageUrl: val.imageUrl || undefined
        }).subscribe({
          next: () => this.handleSuccess('เพิ่มรุ่นสินค้าสำเร็จ'),
          error: err => this.handleError(err)
        });
      } else if (editId) {
        this.masterDataService.updateProduct(editId, {
          brandId: val.brandId!,
          categoryId: val.categoryId!,
          modelName: val.modelName!.trim(),
          productCode: val.productCode!.trim().toUpperCase(),
          description: val.description || undefined,
          defaultSellingPrice: val.defaultSellingPrice || undefined,
          imageUrl: val.imageUrl || undefined,
          isActive: val.isActive ?? true
        }).subscribe({
          next: () => this.handleSuccess('แก้ไขรุ่นสินค้าสำเร็จ'),
          error: err => this.handleError(err)
        });
      }
    } else if (tab === 'customers') {
      if (this.customerForm.invalid) {
        this.customerForm.markAllAsTouched();
        return;
      }
      this.isSubmitting.set(true);
      const val = this.customerForm.getRawValue();

      if (mode === 'create') {
        this.masterDataService.createCustomer({
          customerType: Number(val.customerType),
          name: val.name!.trim(),
          phone: val.phone!.trim(),
          email: val.email || undefined,
          idCardNumber: val.idCardNumber || undefined,
          address: val.address || undefined,
          lineId: val.lineId || undefined,
          facebook: val.facebook || undefined,
          bankName: val.bankName || undefined,
          bankAccountNumber: val.bankAccountNumber || undefined,
          bankAccountName: val.bankAccountName || undefined,
          notes: val.notes || undefined
        }).subscribe({
          next: () => this.handleSuccess('บันทึกข้อมูลลูกค้าสำเร็จ'),
          error: err => this.handleError(err)
        });
      } else if (editId) {
        this.masterDataService.updateCustomer(editId, {
          customerType: Number(val.customerType),
          name: val.name!.trim(),
          phone: val.phone!.trim(),
          email: val.email || undefined,
          idCardNumber: val.idCardNumber || undefined,
          address: val.address || undefined,
          lineId: val.lineId || undefined,
          facebook: val.facebook || undefined,
          bankName: val.bankName || undefined,
          bankAccountNumber: val.bankAccountNumber || undefined,
          bankAccountName: val.bankAccountName || undefined,
          notes: val.notes || undefined,
          isActive: val.isActive ?? true
        }).subscribe({
          next: () => this.handleSuccess('แก้ไขข้อมูลลูกค้าสำเร็จ'),
          error: err => this.handleError(err)
        });
      }
    }
  }

  // ==========================================
  // DELETE ACTIONS
  // ==========================================

  confirmDelete(type: MasterTab, id: string, name: string): void {
    this.itemToDelete.set({ type, id, name });
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
    this.itemToDelete.set(null);
  }

  executeDelete(): void {
    const item = this.itemToDelete();
    if (!item) return;

    this.isSubmitting.set(true);

    let deleteObs$;
    if (item.type === 'brands') deleteObs$ = this.masterDataService.deleteBrand(item.id);
    else if (item.type === 'categories') deleteObs$ = this.masterDataService.deleteCategory(item.id);
    else if (item.type === 'grades') deleteObs$ = this.masterDataService.deleteConditionGrade(item.id);
    else if (item.type === 'products') deleteObs$ = this.masterDataService.deleteProduct(item.id);
    else if (item.type === 'customers') deleteObs$ = this.masterDataService.deleteCustomer(item.id);

    if (deleteObs$) {
      deleteObs$.subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.closeDeleteModal();
          this.feedback.set({ type: 'success', text: `ลบ '${item.name}' สำเร็จ` });
        },
        error: err => {
          this.isSubmitting.set(false);
          this.closeDeleteModal();
          this.handleError(err);
        }
      });
    }
  }

  private handleSuccess(message: string): void {
    this.isSubmitting.set(false);
    this.closeModal();
    this.feedback.set({ type: 'success', text: message });
  }

  private handleError(err: any): void {
    this.isSubmitting.set(false);
    const msg = err.error?.message || err.message || 'เกิดข้อผิดพลาดในการทำรายการ';
    this.feedback.set({ type: 'error', text: msg });
  }
}
