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
    const width = this.actualWidthMm();
    const height = this.actualHeightMm();
    const copies = Math.max(1, Math.min(50, this.printCopies()));

    const previewEl = document.getElementById('sticker-label-preview');
    if (!previewEl) return;

    const stickerHtml = previewEl.innerHTML;

    let labelsHtml = '';
    for (let i = 0; i < copies; i++) {
      labelsHtml += `<div class="sticker-page">${stickerHtml}</div>`;
    }

    let iframe = document.getElementById('sticker-print-iframe') as HTMLIFrameElement;
    if (!iframe) {
      iframe = document.createElement('iframe');
      iframe.id = 'sticker-print-iframe';
      iframe.style.position = 'fixed';
      iframe.style.top = '-9999px';
      iframe.style.left = '-9999px';
      iframe.style.width = '0';
      iframe.style.height = '0';
      iframe.style.border = 'none';
      document.body.appendChild(iframe);
    }

    const iframeDoc = iframe.contentWindow?.document;
    if (!iframeDoc) return;

    iframeDoc.open();
    iframeDoc.write(`
      <!DOCTYPE html>
      <html>
      <head>
        <meta charset="utf-8">
        <title>Print Label - ${this.item()?.serialNumber || this.item()?.sku || 'Sticker'}</title>
        <style>
          @page {
            size: ${width}mm ${height}mm;
            margin: 0;
          }
          * {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
          }
          html, body {
            margin: 0 !important;
            padding: 0 !important;
            background: #ffffff !important;
            -webkit-print-color-adjust: exact;
            print-color-adjust: exact;
          }
          .sticker-page {
            width: ${width}mm;
            height: ${height}mm;
            padding: 1.5mm 2mm;
            page-break-after: always;
            break-after: page;
            display: flex;
            flex-direction: column;
            justify-content: space-between;
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
            background: #ffffff;
            color: #000000;
            overflow: hidden;
            box-sizing: border-box;
          }
          .sticker-top-row {
            display: flex;
            justify-content: space-between;
            align-items: center;
            gap: 2mm;
            line-height: 1.1;
          }
          .store-branding-box {
            display: flex;
            align-items: center;
            gap: 1.5mm;
            min-width: 0;
            flex: 1;
          }
          .store-logo-img {
            width: 7mm;
            height: 7mm;
            object-fit: contain;
            flex-shrink: 0;
          }
          .monochrome-logo {
            filter: grayscale(100%) contrast(200%);
          }
          .store-logo-placeholder {
            font-size: 7pt;
            line-height: 1;
          }
          .store-name-text {
            font-size: 6.5pt;
            font-weight: 700;
            color: #000000;
            text-transform: uppercase;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
          }
          .grade-badge-tag {
            font-size: 5.5pt;
            font-weight: 800;
            background-color: #000000;
            color: #ffffff;
            padding: 0.5mm 1.5mm;
            border-radius: 1.5px;
            text-transform: uppercase;
            flex-shrink: 0;
          }
          .product-title-row {
            font-size: 7pt;
            font-weight: 800;
            color: #000000;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
            line-height: 1.15;
            margin: 0.5mm 0;
          }
          .shutter-row {
            font-size: 5pt;
            color: #334155;
            font-family: monospace;
          }
          .codes-area {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 1.5mm;
            margin: 0.5mm 0;
            flex: 1;
            min-height: 0;
          }
          .codes-area.barcode_only .barcode-box {
            width: 100%;
          }
          .codes-area.qr_only .qr-box {
            width: 100%;
            justify-content: center;
          }
          .barcode-box {
            display: flex;
            flex-direction: column;
            align-items: center;
            flex: 1;
            min-width: 0;
            overflow: hidden;
          }
          .barcode-wrapper {
            display: flex;
            flex-direction: column;
            align-items: center;
            width: 100%;
            max-width: 100%;
            overflow: hidden;
          }
          .barcode-svg {
            display: block;
            max-width: 100%;
            height: auto;
          }
          .qr-box {
            display: flex;
            flex-direction: column;
            align-items: center;
            flex-shrink: 0;
          }
          .qr-box svg {
            display: block;
            max-width: 100%;
            height: auto;
          }
          .code-meta-text {
            display: flex;
            justify-content: space-between;
            width: 100%;
            font-size: 5pt;
            color: #000000;
            font-family: monospace;
            line-height: 1;
            margin-top: 0.5mm;
          }
          .qr-label-text {
            font-size: 4.5pt;
            margin-top: 0.5mm;
            color: #000000;
          }
          .sticker-bottom-row {
            display: flex;
            justify-content: space-between;
            align-items: flex-end;
            border-top: 0.5px solid #000000;
            padding-top: 0.5mm;
            line-height: 1;
          }
          .price-tag-text {
            font-size: 8.5pt;
            font-weight: 900;
            color: #000000;
            letter-spacing: -0.02em;
          }
          .footer-warranty-text {
            font-size: 5pt;
            color: #334155;
            font-weight: 500;
            white-space: nowrap;
          }
        </style>
      </head>
      <body>
        ${labelsHtml}
      </body>
      </html>
    `);
    iframeDoc.close();

    setTimeout(() => {
      iframe.contentWindow?.focus();
      iframe.contentWindow?.print();
    }, 250);
  }
}
