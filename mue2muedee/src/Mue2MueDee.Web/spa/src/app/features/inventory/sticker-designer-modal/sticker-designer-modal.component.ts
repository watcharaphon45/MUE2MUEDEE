import {
  Component,
  input,
  output,
  inject,
  OnInit,
  computed,
  signal,
  ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InventoryItem } from '../inventory.models';
import { AuthService } from '../../../core/auth/auth.service';
import { BarcodeComponent } from '../../../shared/components/barcode/barcode.component';
import { QrcodeComponent } from '../../../shared/components/qrcode/qrcode.component';

export interface LabelTemplateSettings {
  labelSize: '50x30' | '40x30' | '40x20' | '60x40' | '70x50' | 'custom';
  customWidthMm: number;
  customHeightMm: number;
  showStoreLogo: boolean;
  storeLogoUrl: string;
  showStoreName: boolean;
  storeName: string;
  showProductTitle: boolean;
  showConditionGrade: boolean;
  showSerialNumber: boolean;
  showSku: boolean;
  showPrice: boolean;
  showShutterCount: boolean;
  showFooterText: boolean;
  footerText: string;
  codeStyle: 'barcode_qr' | 'barcode_only' | 'qr_only';
  encodeValue: 'serial' | 'sku';
  monochromeLogo: boolean;
  barcodeHeight: number;
  barcodeWidth: number;
  qrSize: number;
  showBarcodeText: boolean;
}

const STORAGE_KEY = 'mue2muedee_label_template';

@Component({
  selector: 'app-sticker-designer-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, BarcodeComponent, QrcodeComponent],
  templateUrl: './sticker-designer-modal.component.html',
  styleUrl: './sticker-designer-modal.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StickerDesignerModalComponent implements OnInit {
  private authService = inject(AuthService);

  item = input<InventoryItem | null>(null);
  isOpen = input<boolean>(false);
  closeModal = output<void>();

  // Settings Signals
  labelSize = signal<'50x30' | '40x30' | '40x20' | '60x40' | '70x50' | 'custom'>('50x30');
  customWidthMm = signal<number>(50);
  customHeightMm = signal<number>(30);

  showStoreLogo = signal<boolean>(true);
  storeLogoUrl = signal<string>('');
  showStoreName = signal<boolean>(true);
  storeName = signal<string>('');

  showProductTitle = signal<boolean>(true);
  showConditionGrade = signal<boolean>(true);
  showSerialNumber = signal<boolean>(true);
  showSku = signal<boolean>(true);
  showPrice = signal<boolean>(true);
  showShutterCount = signal<boolean>(false);
  showFooterText = signal<boolean>(true);
  footerText = signal<string>('รับประกัน 30 วัน');

  codeStyle = signal<'barcode_qr' | 'barcode_only' | 'qr_only'>('barcode_qr');
  encodeValue = signal<'serial' | 'sku'>('serial');
  monochromeLogo = signal<boolean>(true);

  barcodeHeight = signal<number>(30);
  barcodeWidth = signal<number>(1.4);
  qrSize = signal<number>(44);
  showBarcodeText = signal<boolean>(false);

  printCopies = signal<number>(1);
  saveSuccessMessage = signal<string | null>(null);

  // Computed dimensions in mm
  actualWidthMm = computed(() => {
    switch (this.labelSize()) {
      case '40x30': return 40;
      case '40x20': return 40;
      case '60x40': return 60;
      case '70x50': return 70;
      case 'custom': return Math.max(20, this.customWidthMm());
      default: return 50;
    }
  });

  actualHeightMm = computed(() => {
    switch (this.labelSize()) {
      case '40x30': return 30;
      case '40x20': return 20;
      case '60x40': return 40;
      case '70x50': return 50;
      case 'custom': return Math.max(15, this.customHeightMm());
      default: return 30;
    }
  });

  encodedString = computed(() => {
    const cur = this.item();
    if (!cur) return '';
    return this.encodeValue() === 'serial' ? (cur.serialNumber || cur.sku) : cur.sku;
  });

  copiesList = computed(() => {
    const count = Math.max(1, Math.min(50, this.printCopies()));
    return Array.from({ length: count }, (_, i) => i);
  });

  ngOnInit(): void {
    this.loadSavedTemplate();
  }

  loadSavedTemplate(): void {
    const currentStore = this.authService.currentStore();
    const defaultStoreName = currentStore?.storeName ?? 'MUE2MUEDEE';
    const defaultLogoUrl = currentStore?.logoUrl ?? '';

    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      if (saved) {
        const parsed: LabelTemplateSettings = JSON.parse(saved);
        this.labelSize.set(parsed.labelSize ?? '50x30');
        this.customWidthMm.set(parsed.customWidthMm ?? 50);
        this.customHeightMm.set(parsed.customHeightMm ?? 30);
        this.showStoreLogo.set(parsed.showStoreLogo ?? true);
        this.storeLogoUrl.set(parsed.storeLogoUrl || defaultLogoUrl);
        this.showStoreName.set(parsed.showStoreName ?? true);
        this.storeName.set(parsed.storeName || defaultStoreName);
        this.showProductTitle.set(parsed.showProductTitle ?? true);
        this.showConditionGrade.set(parsed.showConditionGrade ?? true);
        this.showSerialNumber.set(parsed.showSerialNumber ?? true);
        this.showSku.set(parsed.showSku ?? true);
        this.showPrice.set(parsed.showPrice ?? true);
        this.showShutterCount.set(parsed.showShutterCount ?? false);
        this.showFooterText.set(parsed.showFooterText ?? true);
        this.footerText.set(parsed.footerText ?? 'รับประกัน 30 วัน');
        this.codeStyle.set(parsed.codeStyle ?? 'barcode_qr');
        this.encodeValue.set(parsed.encodeValue ?? 'serial');
        this.monochromeLogo.set(parsed.monochromeLogo ?? true);
        this.barcodeHeight.set(parsed.barcodeHeight ?? 30);
        this.barcodeWidth.set(parsed.barcodeWidth ?? 1.4);
        this.qrSize.set(parsed.qrSize ?? 44);
        this.showBarcodeText.set(parsed.showBarcodeText ?? false);
        return;
      }
    } catch {
      // Fallback
    }

    this.storeName.set(defaultStoreName);
    this.storeLogoUrl.set(defaultLogoUrl);
  }

  saveAsDefaultTemplate(): void {
    const settings: LabelTemplateSettings = {
      labelSize: this.labelSize(),
      customWidthMm: this.customWidthMm(),
      customHeightMm: this.customHeightMm(),
      showStoreLogo: this.showStoreLogo(),
      storeLogoUrl: this.storeLogoUrl(),
      showStoreName: this.showStoreName(),
      storeName: this.storeName(),
      showProductTitle: this.showProductTitle(),
      showConditionGrade: this.showConditionGrade(),
      showSerialNumber: this.showSerialNumber(),
      showSku: this.showSku(),
      showPrice: this.showPrice(),
      showShutterCount: this.showShutterCount(),
      showFooterText: this.showFooterText(),
      footerText: this.footerText(),
      codeStyle: this.codeStyle(),
      encodeValue: this.encodeValue(),
      monochromeLogo: this.monochromeLogo(),
      barcodeHeight: this.barcodeHeight(),
      barcodeWidth: this.barcodeWidth(),
      qrSize: this.qrSize(),
      showBarcodeText: this.showBarcodeText()
    };

    localStorage.setItem(STORAGE_KEY, JSON.stringify(settings));
    this.saveSuccessMessage.set('บันทึกการตั้งค่าเทมเพลตเริ่มต้นสำเร็จแล้ว!');
    setTimeout(() => this.saveSuccessMessage.set(null), 3000);
  }

  resetToDefaults(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.labelSize.set('50x30');
    this.customWidthMm.set(50);
    this.customHeightMm.set(30);
    this.showStoreLogo.set(true);
    this.showStoreName.set(true);
    this.showProductTitle.set(true);
    this.showConditionGrade.set(true);
    this.showSerialNumber.set(true);
    this.showSku.set(true);
    this.showPrice.set(true);
    this.showShutterCount.set(false);
    this.showFooterText.set(true);
    this.footerText.set('รับประกัน 30 วัน');
    this.codeStyle.set('barcode_qr');
    this.encodeValue.set('serial');
    this.monochromeLogo.set(true);
    this.barcodeHeight.set(30);
    this.barcodeWidth.set(1.4);
    this.qrSize.set(44);
    this.showBarcodeText.set(false);

    const currentStore = this.authService.currentStore();
    this.storeName.set(currentStore?.storeName ?? 'MUE2MUEDEE');
    this.storeLogoUrl.set(currentStore?.logoUrl ?? '');
  }

  print(): void {
    // Inject dynamic print style for exact label dimensions
    const width = this.actualWidthMm();
    const height = this.actualHeightMm();

    let styleEl = document.getElementById('sticker-print-style') as HTMLStyleElement;
    if (!styleEl) {
      styleEl = document.createElement('style');
      styleEl.id = 'sticker-print-style';
      document.head.appendChild(styleEl);
    }

    styleEl.innerHTML = `
      @page {
        size: ${width}mm ${height}mm;
        margin: 0;
      }
      @media print {
        html, body {
          width: ${width}mm !important;
          height: auto !important;
          margin: 0 !important;
          padding: 0 !important;
          background: #ffffff !important;
        }
      }
    `;

    window.print();
  }
}
