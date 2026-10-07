import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { SaleService } from '../sale.service';
import { PaymentMethod } from '../sale.models';
import { InventoryItem } from '../../inventory/inventory.models';
import {
  TextboxComponent,
  NumberComponent,
  SelectComponent,
  StatusBadgeComponent
} from '../../../shared/components';

interface CartItem {
  inventoryItem: InventoryItem;
  unitPrice: number;
  discount: number;
  subTotal: number;
  grossProfit: number;
  marginPercentage: number;
  warrantyDays: number;
}

interface PaymentEntry {
  method: PaymentMethod;
  methodName: string;
  amount: number;
  referenceNumber?: string;
  notes?: string;
}

@Component({
  selector: 'app-pos',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    TextboxComponent,
    NumberComponent,
    SelectComponent
  ],
  templateUrl: './pos.component.html',
  styleUrl: './pos.component.css'
})
export class PosComponent implements OnInit {
  protected readonly saleService = inject(SaleService);
  private readonly router = inject(Router);

  // Search & Results
  searchQuery = new FormControl('');
  searchResults = signal<InventoryItem[]>([]);
  isSearching = signal<boolean>(false);

  // Cart
  cartItems = signal<CartItem[]>([]);

  // Customer Form
  customerForm = new FormGroup({
    customerName: new FormControl('ลูกค้าหน้าร้าน (Walk-in)', [Validators.required]),
    customerPhone: new FormControl(''),
    notes: new FormControl('')
  });

  // Payment Form for adding a payment tender
  paymentForm = new FormGroup({
    paymentMethod: new FormControl<string>(PaymentMethod.PromptPay.toString(), [Validators.required]),
    amount: new FormControl<number | null>(null, [Validators.required, Validators.min(1)]),
    referenceNumber: new FormControl(''),
    notes: new FormControl('')
  });

  payments = signal<PaymentEntry[]>([]);

  isSubmitting = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  successSaleNumber = signal<string | null>(null);
  createdSaleId = signal<string | null>(null);

  readonly paymentMethodOptions = [
    { value: PaymentMethod.PromptPay.toString(), label: 'พร้อมเพย์ QR (PromptPay)' },
    { value: PaymentMethod.BankTransfer.toString(), label: 'โอนเงินธนาคาร (Bank Transfer)' },
    { value: PaymentMethod.Cash.toString(), label: 'เงินสด (Cash)' },
    { value: PaymentMethod.CreditCard.toString(), label: 'บัตรเครดิต (Credit Card)' },
    { value: PaymentMethod.Other.toString(), label: 'อื่นๆ' }
  ];

  // Computed Totals
  totalAmount = computed(() => {
    return this.cartItems().reduce((acc, item) => acc + item.unitPrice, 0);
  });

  totalDiscount = computed(() => {
    return this.cartItems().reduce((acc, item) => acc + item.discount, 0);
  });

  netAmount = computed(() => {
    return this.cartItems().reduce((acc, item) => acc + item.subTotal, 0);
  });

  totalCost = computed(() => {
    return this.cartItems().reduce((acc, item) => acc + item.inventoryItem.actualCost, 0);
  });

  totalGrossProfit = computed(() => {
    return this.netAmount() - this.totalCost();
  });

  totalMarginPercentage = computed(() => {
    const net = this.netAmount();
    return net > 0 ? Math.round((this.totalGrossProfit() / net) * 10000) / 100 : 0;
  });

  totalPaid = computed(() => {
    return this.payments().reduce((acc, p) => acc + p.amount, 0);
  });

  remainingAmount = computed(() => {
    return Math.max(0, this.netAmount() - this.totalPaid());
  });

  changeAmount = computed(() => {
    return Math.max(0, this.totalPaid() - this.netAmount());
  });

  ngOnInit(): void {
    // Search on value changes
    this.searchQuery.valueChanges.subscribe(q => {
      if (q && q.trim().length >= 2) {
        this.performSearch(q.trim());
      } else {
        this.searchResults.set([]);
      }
    });
  }

  performSearch(query: string): void {
    this.isSearching.set(true);
    this.saleService.searchAvailableItems(query).subscribe({
      next: items => {
        // Exclude items already in cart
        const inCartIds = new Set(this.cartItems().map(c => c.inventoryItem.id));
        this.searchResults.set(items.filter(i => !inCartIds.has(i.id)));
        this.isSearching.set(false);
      },
      error: () => this.isSearching.set(false)
    });
  }

  addToCart(item: InventoryItem): void {
    const unitPrice = item.sellingPrice;
    const discount = 0;
    const subTotal = unitPrice - discount;
    const grossProfit = subTotal - item.actualCost;
    const margin = subTotal > 0 ? Math.round((grossProfit / subTotal) * 10000) / 100 : 0;

    const cartItem: CartItem = {
      inventoryItem: item,
      unitPrice,
      discount,
      subTotal,
      grossProfit,
      marginPercentage: margin,
      warrantyDays: 30
    };

    this.cartItems.update(items => [...items, cartItem]);
    this.searchResults.update(res => res.filter(r => r.id !== item.id));
    this.searchQuery.setValue('');

    // Pre-fill payment amount if no payment yet
    if (this.payments().length === 0) {
      this.paymentForm.controls.amount.setValue(this.netAmount());
    } else {
      this.paymentForm.controls.amount.setValue(this.remainingAmount());
    }
  }

  updateItemDiscount(index: number, discount: number): void {
    this.cartItems.update(items => {
      const copy = [...items];
      const target = { ...copy[index] };
      target.discount = Math.max(0, discount);
      target.subTotal = Math.max(0, target.unitPrice - target.discount);
      target.grossProfit = target.subTotal - target.inventoryItem.actualCost;
      target.marginPercentage = target.subTotal > 0 ? Math.round((target.grossProfit / target.subTotal) * 10000) / 100 : 0;
      copy[index] = target;
      return copy;
    });

    this.updatePaymentSuggestion();
  }

  removeFromCart(index: number): void {
    this.cartItems.update(items => items.filter((_, i) => i !== index));
    this.updatePaymentSuggestion();
  }

  updatePaymentSuggestion(): void {
    if (this.payments().length === 0) {
      this.paymentForm.controls.amount.setValue(this.netAmount());
    } else {
      this.paymentForm.controls.amount.setValue(this.remainingAmount());
    }
  }

  addPayment(): void {
    if (this.paymentForm.invalid) {
      this.paymentForm.markAllAsTouched();
      return;
    }

    const val = this.paymentForm.getRawValue();
    const method = parseInt(val.paymentMethod!, 10) as PaymentMethod;
    const methodName = this.paymentMethodOptions.find(o => o.value === val.paymentMethod)?.label ?? 'อื่นๆ';

    const entry: PaymentEntry = {
      method,
      methodName,
      amount: val.amount!,
      referenceNumber: val.referenceNumber || undefined,
      notes: val.notes || undefined
    };

    this.payments.update(p => [...p, entry]);
    this.paymentForm.controls.amount.setValue(this.remainingAmount());
    this.paymentForm.controls.referenceNumber.setValue('');
    this.paymentForm.controls.notes.setValue('');
  }

  removePayment(index: number): void {
    this.payments.update(p => p.filter((_, i) => i !== index));
    this.paymentForm.controls.amount.setValue(this.remainingAmount());
  }

  submitSale(): void {
    if (this.cartItems().length === 0) {
      this.errorMessage.set('กรุณาเลือกสินค้าลงตะกร้าก่อนทำการขาย');
      return;
    }

    if (this.totalPaid() < this.netAmount()) {
      this.errorMessage.set(`ยอดชำระเงินยังไม่ครบ ยังขาดอีก ฿${this.remainingAmount().toLocaleString('th-TH', { minimumFractionDigits: 2 })}`);
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const customerVal = this.customerForm.getRawValue();

    const request = {
      customerName: customerVal.customerName || undefined,
      customerPhone: customerVal.customerPhone || undefined,
      notes: customerVal.notes || undefined,
      items: this.cartItems().map(item => ({
        inventoryItemId: item.inventoryItem.id,
        unitPrice: item.unitPrice,
        discount: item.discount,
        warrantyDays: item.warrantyDays
      })),
      payments: this.payments().map(p => ({
        paymentMethod: p.method,
        amount: p.amount,
        referenceNumber: p.referenceNumber,
        notes: p.notes
      }))
    };

    this.saleService.createSale(request).subscribe({
      next: sale => {
        this.isSubmitting.set(false);
        this.successSaleNumber.set(sale.saleNumber);
        this.createdSaleId.set(sale.id);
      },
      error: err => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.error?.message ?? 'เกิดข้อผิดพลาดในการบันทึกการขาย');
      }
    });
  }

  resetForm(): void {
    this.cartItems.set([]);
    this.payments.set([]);
    this.customerForm.reset({
      customerName: 'ลูกค้าหน้าร้าน (Walk-in)',
      customerPhone: '',
      notes: ''
    });
    this.searchQuery.setValue('');
    this.successSaleNumber.set(null);
    this.createdSaleId.set(null);
    this.errorMessage.set(null);
  }

  goToDetail(): void {
    if (this.createdSaleId()) {
      this.router.navigate(['/sales', this.createdSaleId()]);
    }
  }
}
