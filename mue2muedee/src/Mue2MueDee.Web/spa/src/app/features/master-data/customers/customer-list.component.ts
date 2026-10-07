import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MasterDataService } from '../master-data.service';
import { Customer } from '../master-data.models';
import {
  TextboxComponent,
  TextareaComponent,
  SelectComponent
} from '../../../shared/components';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TextboxComponent,
    TextareaComponent,
    SelectComponent
  ],
  templateUrl: './customer-list.component.html',
  styleUrl: './customer-list.component.css'
})
export class CustomerListComponent implements OnInit {
  protected readonly masterDataService = inject(MasterDataService);

  showCreateModal = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);
  feedback = signal<{ type: 'success' | 'error'; text: string } | null>(null);

  searchControl = new FormControl('');

  readonly customerTypeOptions = [
    { value: '3', label: 'ซื้อและขาย (Both)' },
    { value: '1', label: 'ผู้ขายของเข้า (Seller)' },
    { value: '2', label: 'ลูกค้าซื้อสินค้า (Buyer)' }
  ];

  form = new FormGroup({
    name: new FormControl('', [Validators.required]),
    phone: new FormControl('', [Validators.required]),
    customerType: new FormControl('3', [Validators.required]),
    idCardNumber: new FormControl(''),
    email: new FormControl(''),
    lineId: new FormControl(''),
    bankName: new FormControl(''),
    bankAccountNumber: new FormControl(''),
    bankAccountName: new FormControl(''),
    address: new FormControl(''),
    notes: new FormControl('')
  });

  ngOnInit(): void {
    this.masterDataService.loadCustomers().subscribe();

    this.searchControl.valueChanges.subscribe(val => {
      this.masterDataService.loadCustomers(val || undefined).subscribe();
    });
  }

  openCreate(): void {
    this.form.reset({ customerType: '3' });
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

    this.masterDataService.createCustomer({
      name: val.name!.trim(),
      phone: val.phone!.trim(),
      customerType: parseInt(val.customerType!, 10),
      idCardNumber: val.idCardNumber || undefined,
      email: val.email || undefined,
      lineId: val.lineId || undefined,
      bankName: val.bankName || undefined,
      bankAccountNumber: val.bankAccountNumber || undefined,
      bankAccountName: val.bankAccountName || undefined,
      address: val.address || undefined,
      notes: val.notes || undefined
    }).subscribe({
      next: created => {
        this.isSubmitting.set(false);
        this.closeCreate();
        this.feedback.set({ type: 'success', text: `บันทึกข้อมูลลูกค้า '${created.name}' สำเร็จ` });
      },
      error: err => {
        this.isSubmitting.set(false);
        this.feedback.set({ type: 'error', text: err.error?.message ?? 'เกิดข้อผิดพลาดในการบันทึกข้อมูลลูกค้า' });
      }
    });
  }
}
