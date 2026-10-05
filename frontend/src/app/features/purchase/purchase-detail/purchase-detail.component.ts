import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { PurchaseService } from '../purchase.service';
import { PaymentMethod } from '../purchase.models';
import {
  TextboxComponent,
  NumberComponent,
  SelectComponent,
  StatusBadgeComponent
} from '../../../shared/components';

@Component({
  selector: 'app-purchase-detail',
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
  templateUrl: './purchase-detail.component.html',
  styleUrl: './purchase-detail.component.css'
})
export class PurchaseDetailComponent implements OnInit {
  protected readonly purchaseService = inject(PurchaseService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  poId = signal<string>('');
  showPaymentModal = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);
  feedback = signal<{ type: 'success' | 'error'; text: string } | null>(null);

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
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.poId.set(id);
      this.loadDetails(id);
    }
  }

  printReceipt(): void {
    window.print();
  }

  loadDetails(id: string): void {
    this.purchaseService.getOrderById(id).subscribe({
      next: po => {
        this.paymentForm.controls.amount.setValue(po.totalAgreedPrice || po.totalOfferedPrice);
      }
    });
  }

  approvePurchase(): void {
    this.isSubmitting.set(true);
    this.purchaseService.approvePurchase(this.poId()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.feedback.set({ type: 'success', text: 'อนุมัติการรับซื้อราคาพิเศษเรียบร้อยแล้ว' });
      },
      error: err => {
        this.isSubmitting.set(false);
        this.feedback.set({ type: 'error', text: err.error?.message ?? 'ไม่สามารถอนุมัติได้' });
      }
    });
  }

  acceptOffer(): void {
    this.isSubmitting.set(true);
    this.purchaseService.acceptOffer(this.poId()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.feedback.set({ type: 'success', text: 'ลูกค้าตกลงรับข้อเสนอเรียบร้อยแล้ว' });
      },
      error: err => {
        this.isSubmitting.set(false);
        this.feedback.set({ type: 'error', text: err.error?.message ?? 'เกิดข้อผิดพลาดในการตอบรับข้อเสนอ' });
      }
    });
  }

  rejectOffer(): void {
    this.isSubmitting.set(true);
    this.purchaseService.rejectOffer(this.poId()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.feedback.set({ type: 'success', text: 'บันทึกการยกเลิก/ปฏิเสธข้อเสนอเรียบร้อย' });
      },
      error: err => {
        this.isSubmitting.set(false);
        this.feedback.set({ type: 'error', text: err.error?.message ?? 'เกิดข้อผิดพลาด' });
      }
    });
  }

  openPayment(): void {
    const po = this.purchaseService.selectedOrder();
    if (po) {
      this.paymentForm.controls.amount.setValue(po.totalAgreedPrice);
    }
    this.showPaymentModal.set(true);
  }

  closePayment(): void {
    this.showPaymentModal.set(false);
  }

  submitPayment(): void {
    if (this.paymentForm.invalid) {
      this.paymentForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const val = this.paymentForm.getRawValue();

    this.purchaseService.recordPayment(this.poId(), {
      paymentMethod: parseInt(val.paymentMethod!, 10) as PaymentMethod,
      amount: val.amount!,
      referenceNumber: val.referenceNumber || undefined,
      notes: val.notes || undefined
    }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.closePayment();
        this.feedback.set({ type: 'success', text: `บันทึกการจ่ายเงิน ฿${val.amount} สำเร็จ` });
      },
      error: err => {
        this.isSubmitting.set(false);
        this.feedback.set({ type: 'error', text: err.error?.message ?? 'เกิดข้อผิดพลาดในการจ่ายเงิน' });
      }
    });
  }

  receiveToStock(): void {
    this.isSubmitting.set(true);
    this.purchaseService.receiveToStock(this.poId()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.feedback.set({ type: 'success', text: 'นำสินค้าเข้าสต็อกในคลังสินค้าเรียบร้อยแล้ว' });
      },
      error: err => {
        this.isSubmitting.set(false);
        this.feedback.set({ type: 'error', text: err.error?.message ?? 'เกิดข้อผิดพลาดในการนำเข้าสต็อก' });
      }
    });
  }
}
