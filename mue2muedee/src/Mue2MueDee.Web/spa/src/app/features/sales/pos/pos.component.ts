import {
  Component,
  OnInit,
  OnDestroy,
  inject,
  signal,
  computed,
  ViewChild,
  ElementRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { BrowserMultiFormatReader, IScannerControls } from '@zxing/browser';
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
    FormsModule,
    ReactiveFormsModule,
    TextboxComponent,
    NumberComponent,
    SelectComponent
  ],
  templateUrl: './pos.component.html',
  styleUrl: './pos.component.css'
})
export class PosComponent implements OnInit, OnDestroy {
  protected readonly saleService = inject(SaleService);
  private readonly router = inject(Router);

  @ViewChild('scanVideo') scanVideoRef?: ElementRef<HTMLVideoElement>;
  @ViewChild('searchInputRef') searchInputRef?: ElementRef<HTMLInputElement>;

  // Search & Results
  searchQuery = new FormControl('');
  searchResults = signal<InventoryItem[]>([]);
  isSearching = signal<boolean>(false);

  // Scanning Feedback & State
  scanFeedback = signal<{ type: 'success' | 'error'; text: string } | null>(null);
  showCameraScanner = signal<boolean>(false);
  cameraError = signal<string | null>(null);
  availableCameras = signal<MediaDeviceInfo[]>([]);
  currentCameraIndex = signal<number>(0);
  isTorchOn = signal<boolean>(false);
  supportsTorch = signal<boolean>(false);
  continuousScan = signal<boolean>(false);

  private codeReader: BrowserMultiFormatReader | null = null;
  private scannerControls: IScannerControls | null = null;
  private lastScannedCode = '';
  private lastScannedTime = 0;

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

  // AUDIO & VIBRATION FEEDBACK
  playBeepSound(type: 'success' | 'error' = 'success'): void {
    try {
      const AudioCtx = window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext;
      if (!AudioCtx) return;
      const ctx = new AudioCtx();
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      osc.connect(gain);
      gain.connect(ctx.destination);
      if (type === 'success') {
        osc.frequency.value = 1450;
        gain.gain.setValueAtTime(0.18, ctx.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.12);
        osc.start(ctx.currentTime);
        osc.stop(ctx.currentTime + 0.12);
      } else {
        osc.frequency.value = 320;
        gain.gain.setValueAtTime(0.25, ctx.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.28);
        osc.start(ctx.currentTime);
        osc.stop(ctx.currentTime + 0.28);
      }
    } catch {
      // AudioContext policy
    }
  }

  vibrateDevice(): void {
    if (typeof navigator !== 'undefined' && 'vibrate' in navigator) {
      try {
        navigator.vibrate([80]);
      } catch {
        // Ignored
      }
    }
  }

  // HARDWARE BARCODE SCANNER (Auto-enter support)
  onSearchEnter(event?: Event): void {
    if (event) event.preventDefault();
    const query = this.searchQuery.value?.trim();
    if (!query) return;

    this.isSearching.set(true);
    this.saleService.searchAvailableItems(query).subscribe({
      next: items => {
        const inCartIds = new Set(this.cartItems().map(c => c.inventoryItem.id));
        const available = items.filter(i => !inCartIds.has(i.id));

        const exactMatch = available.find(
          i => i.serialNumber.toLowerCase() === query.toLowerCase() ||
               i.sku.toLowerCase() === query.toLowerCase()
        );

        if (exactMatch || available.length === 1) {
          const target = exactMatch || available[0];
          this.addToCart(target);
          this.playBeepSound('success');
          this.vibrateDevice();
          this.scanFeedback.set({
            type: 'success',
            text: `ยิงบาร์โค้ดสำเร็จ: ${target.brandName} ${target.productModelName} [SN: ${target.serialNumber}]`
          });
          setTimeout(() => this.scanFeedback.set(null), 3000);
        } else if (available.length > 1) {
          this.searchResults.set(available);
          this.scanFeedback.set({
            type: 'success',
            text: `พบสินค้า ${available.length} รายการ กรุณาเลือกรายการที่ต้องการ`
          });
          setTimeout(() => this.scanFeedback.set(null), 3000);
        } else {
          this.playBeepSound('error');
          this.scanFeedback.set({
            type: 'error',
            text: `ไม่พบสินค้าพร้อมขายสำหรับรหัส "${query}" หรือสินค้านี้อยู่ในตะกร้าแล้ว`
          });
          setTimeout(() => this.scanFeedback.set(null), 4000);
        }
        this.isSearching.set(false);
      },
      error: () => {
        this.isSearching.set(false);
        this.playBeepSound('error');
      }
    });
  }

  // CAMERA SCANNER (Mobile / Tablet / Webcam)
  async openCameraScanner(): Promise<void> {
    this.showCameraScanner.set(true);
    this.cameraError.set(null);
    this.lastScannedCode = '';

    try {
      this.codeReader = new BrowserMultiFormatReader();
      const videoInputDevices = await BrowserMultiFormatReader.listVideoInputDevices();
      this.availableCameras.set(videoInputDevices);

      // Prefer back camera (environment) if available
      let selectedIndex = 0;
      const backIndex = videoInputDevices.findIndex(d =>
        d.label.toLowerCase().includes('back') ||
        d.label.toLowerCase().includes('rear') ||
        d.label.toLowerCase().includes('environment')
      );
      if (backIndex !== -1) {
        selectedIndex = backIndex;
      }
      this.currentCameraIndex.set(selectedIndex);

      // Allow DOM to render video element before binding
      setTimeout(() => this.startScanning(), 100);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'ไม่สามารถเข้าถึงกล้องได้';
      this.cameraError.set(`ข้อผิดพลาดกล้อง: ${msg} (โปรดตรวจสอบสิทธิ์การอนุญาตใช้กล้อง)`);
    }
  }

  async startScanning(): Promise<void> {
    const videoEl = this.scanVideoRef?.nativeElement;
    if (!videoEl || !this.codeReader) return;

    const devices = this.availableCameras();
    const deviceId = devices.length > 0 ? devices[this.currentCameraIndex()].deviceId : undefined;

    try {
      this.stopScanning();
      this.scannerControls = await this.codeReader.decodeFromVideoDevice(
        deviceId,
        videoEl,
        (result, error) => {
          if (result) {
            this.handleScannedCode(result.getText().trim());
          }
        }
      );
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'เกิดข้อผิดพลาดในการเปิดกล้อง';
      this.cameraError.set(msg);
    }
  }

  handleScannedCode(code: string): void {
    if (!code) return;
    const now = Date.now();
    // Debounce duplicate scans within 1.8 seconds
    if (code === this.lastScannedCode && (now - this.lastScannedTime) < 1800) {
      return;
    }
    this.lastScannedCode = code;
    this.lastScannedTime = now;

    this.saleService.searchAvailableItems(code).subscribe({
      next: items => {
        const inCartIds = new Set(this.cartItems().map(c => c.inventoryItem.id));
        const available = items.filter(i => !inCartIds.has(i.id));

        const exactMatch = available.find(
          i => i.serialNumber.toLowerCase() === code.toLowerCase() ||
               i.sku.toLowerCase() === code.toLowerCase()
        );

        if (exactMatch || available.length === 1) {
          const target = exactMatch || available[0];
          this.addToCart(target);
          this.playBeepSound('success');
          this.vibrateDevice();

          this.scanFeedback.set({
            type: 'success',
            text: `สแกนสำเร็จ: ${target.brandName} ${target.productModelName} [SN: ${target.serialNumber}]`
          });
          setTimeout(() => this.scanFeedback.set(null), 3000);

          if (!this.continuousScan()) {
            this.closeCameraScanner();
          }
        } else {
          this.playBeepSound('error');
          this.scanFeedback.set({
            type: 'error',
            text: `ไม่พบสินค้าพร้อมขายสำหรับรหัส "${code}"`
          });
          setTimeout(() => this.scanFeedback.set(null), 3500);
        }
      },
      error: () => this.playBeepSound('error')
    });
  }

  stopScanning(): void {
    if (this.scannerControls) {
      this.scannerControls.stop();
      this.scannerControls = null;
    }
  }

  closeCameraScanner(): void {
    this.stopScanning();
    this.showCameraScanner.set(false);
    this.isTorchOn.set(false);
  }

  switchCamera(): void {
    const devices = this.availableCameras();
    if (devices.length <= 1) return;
    const nextIdx = (this.currentCameraIndex() + 1) % devices.length;
    this.currentCameraIndex.set(nextIdx);
    this.startScanning();
  }

  toggleTorch(): void {
    // Torch toggle via track constraints if supported
    try {
      const stream = this.scanVideoRef?.nativeElement?.srcObject as MediaStream;
      const track = stream?.getVideoTracks()[0];
      if (track && 'applyConstraints' in track) {
        const nextState = !this.isTorchOn();
        track.applyConstraints({
          advanced: [{ torch: nextState } as unknown as MediaTrackConstraintSet]
        }).then(() => {
          this.isTorchOn.set(nextState);
        }).catch(() => {
          // Unsupported torch
        });
      }
    } catch {
      // Torch not supported
    }
  }

  ngOnDestroy(): void {
    this.closeCameraScanner();
  }
}
