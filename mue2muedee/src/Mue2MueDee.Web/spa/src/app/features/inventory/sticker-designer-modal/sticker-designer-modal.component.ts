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
import JsBarcode from 'jsbarcode';
import QRCode from 'qrcode';
import { InventoryItem } from '../inventory.models';
import { AuthService } from '../../../core/auth/auth.service';
import { StickerTemplateService } from '../sticker-template.service';
import {
  StickerTemplate,
  CreateStickerTemplateRequest,
  UpdateStickerTemplateRequest
} from '../sticker-template.models';
import { BarcodeComponent } from '../../../shared/components/barcode/barcode.component';
import { QrcodeComponent } from '../../../shared/components/qrcode/qrcode.component';

@Component({
  selector: 'app-sticker-designer-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, BarcodeComponent, QrcodeComponent],
  templateUrl: './sticker-designer-modal.component.html',
  styleUrl: './sticker-designer-modal.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StickerDesignerModalComponent implements OnInit {
  protected readonly templateService = inject(StickerTemplateService);
  private readonly authService = inject(AuthService);

  // Inputs & Outputs
  item = input<InventoryItem | null>(null);
  items = input<InventoryItem[]>([]);
  isOpen = input<boolean>(false);
  closeModal = output<void>();

  // Active Tab: 'layout' | 'content' | 'codes' | 'calibration'
  activeTab = signal<'layout' | 'content' | 'codes' | 'calibration'>('layout');

  // Batch items computation
  allItems = computed<InventoryItem[]>(() => {
    const batch = this.items();
    if (batch && batch.length > 0) return batch;
    const single = this.item();
    return single ? [single] : [];
  });

  // Active item for single preview
  activeItemIndex = signal<number>(0);
  activeItem = computed<InventoryItem | null>(() => {
    const list = this.allItems();
    if (list.length === 0) return null;
    const idx = Math.min(Math.max(0, this.activeItemIndex()), list.length - 1);
    return list[idx];
  });

  // Current Template Identity
  currentTemplateId = signal<string | null>(null);
  templateName = signal<string>('ม้วนเดี่ยวมาตรฐาน (50x30 mm)');
  templateDescription = signal<string>('');
  isDefault = signal<boolean>(false);

  // Paper & Dimension Settings
  isContinuousRoll = signal<boolean>(true);
  paperWidthMm = signal<number>(50);
  paperHeightMm = signal<number | null>(null);

  labelWidthMm = signal<number>(50);
  labelHeightMm = signal<number>(30);

  columnsCount = signal<number>(1);
  rowsCount = signal<number | null>(null);
  horizontalGapMm = signal<number>(0);
  verticalGapMm = signal<number>(2);

  marginTopMm = signal<number>(0);
  marginBottomMm = signal<number>(0);
  marginLeftMm = signal<number>(0);
  marginRightMm = signal<number>(0);

  // Content Visibility Signals
  showStoreLogo = signal<boolean>(true);
  storeLogoUrl = signal<string>('/logo.png');
  monochromeLogo = signal<boolean>(true);

  showStoreName = signal<boolean>(true);
  storeName = signal<string>('MUE2MUEDEE');

  showProductTitle = signal<boolean>(true);
  showConditionGrade = signal<boolean>(true);
  showSerialNumber = signal<boolean>(true);
  showSku = signal<boolean>(true);
  showPrice = signal<boolean>(true);
  showShutterCount = signal<boolean>(false);
  showFooterText = signal<boolean>(true);
  footerText = signal<string>('รับประกัน 30 วัน');

  // Barcode & QR Code Settings
  codeStyle = signal<'barcode_qr' | 'barcode_only' | 'qr_only'>('barcode_qr');
  encodeValue = signal<'serial' | 'sku'>('serial');
  barcodeHeightMm = signal<number>(6);
  barcodeWidthRatio = signal<number>(1.2);
  qrSizeMm = signal<number>(10);
  showBarcodeText = signal<boolean>(false);

  // Print controls
  printCopies = signal<number>(1);
  statusMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  // Validation Computed
  totalRequiredWidthMm = computed(() => {
    const cols = Math.max(1, this.columnsCount());
    const labelW = this.labelWidthMm();
    const horizG = this.horizontalGapMm();
    const mLeft = this.marginLeftMm();
    const mRight = this.marginRightMm();

    return (cols * labelW) + ((cols - 1) * horizG) + mLeft + mRight;
  });

  isWidthExceeded = computed(() => {
    return this.totalRequiredWidthMm() > this.paperWidthMm();
  });

  totalStickerQuantity = computed(() => {
    const itemCount = this.allItems().length || 1;
    return itemCount * Math.max(1, this.printCopies());
  });

  ngOnInit(): void {
    this.loadTemplatesAndApplyDefault();
  }

  loadTemplatesAndApplyDefault(): void {
    this.templateService.loadTemplates().subscribe({
      next: templates => {
        if (templates.length > 0) {
          const def = templates.find(t => t.isDefault) || templates[0];
          this.applyTemplate(def);
        } else {
          // If no templates exist, initialize with store details
          const curStore = this.authService.currentStore();
          this.storeName.set(curStore?.storeName ?? 'MUE2MUEDEE');
          this.storeLogoUrl.set(curStore?.logoUrl || '/logo.png');
        }
      }
    });
  }

  onSelectTemplate(event: Event): void {
    const select = event.target as HTMLSelectElement;
    const id = select.value;
    const template = this.templateService.templates().find(t => t.id === id);
    if (template) {
      this.applyTemplate(template);
    }
  }

  applyTemplate(t: StickerTemplate): void {
    this.currentTemplateId.set(t.id);
    this.templateName.set(t.name);
    this.templateDescription.set(t.description || '');
    this.isDefault.set(t.isDefault);

    this.isContinuousRoll.set(t.isContinuousRoll);
    this.paperWidthMm.set(t.paperWidthMm);
    this.paperHeightMm.set(t.paperHeightMm ?? null);

    this.labelWidthMm.set(t.labelWidthMm);
    this.labelHeightMm.set(t.labelHeightMm);

    this.columnsCount.set(t.columnsCount);
    this.rowsCount.set(t.rowsCount ?? null);
    this.horizontalGapMm.set(t.horizontalGapMm);
    this.verticalGapMm.set(t.verticalGapMm);

    this.marginTopMm.set(t.marginTopMm);
    this.marginBottomMm.set(t.marginBottomMm);
    this.marginLeftMm.set(t.marginLeftMm);
    this.marginRightMm.set(t.marginRightMm);

    this.showStoreLogo.set(t.showStoreLogo);
    this.storeLogoUrl.set(t.storeLogoUrl || '/logo.png');
    this.monochromeLogo.set(t.monochromeLogo);

    this.showStoreName.set(t.showStoreName);
    this.storeName.set(t.storeName || this.authService.currentStore()?.storeName || 'MUE2MUEDEE');

    this.showProductTitle.set(t.showProductTitle);
    this.showConditionGrade.set(t.showConditionGrade);
    this.showSerialNumber.set(t.showSerialNumber);
    this.showSku.set(t.showSku);
    this.showPrice.set(t.showPrice);
    this.showShutterCount.set(t.showShutterCount);
    this.showFooterText.set(t.showFooterText);
    this.footerText.set(t.footerText || '');

    this.codeStyle.set(t.codeStyle);
    this.encodeValue.set(t.encodeValue);

    this.barcodeHeightMm.set(t.barcodeHeightMm);
    this.barcodeWidthRatio.set(t.barcodeWidthRatio);
    this.qrSizeMm.set(t.qrSizeMm);
    this.showBarcodeText.set(t.showBarcodeText);

    this.clearMessages();
  }

  // Quick preset selector
  applyQuickPreset(type: '1col_roll' | '2col_roll' | '3col_roll' | 'a4_3x8'): void {
    switch (type) {
      case '1col_roll':
        this.templateName.set('ม้วนเดี่ยวมาตรฐาน (1 คอลัมน์ - 50x30 mm)');
        this.columnsCount.set(1);
        this.rowsCount.set(null);
        this.isContinuousRoll.set(true);
        this.paperWidthMm.set(50);
        this.paperHeightMm.set(null);
        this.labelWidthMm.set(50);
        this.labelHeightMm.set(30);
        this.horizontalGapMm.set(0);
        this.verticalGapMm.set(2);
        this.marginLeftMm.set(0);
        this.marginRightMm.set(0);
        this.marginTopMm.set(0);
        this.marginBottomMm.set(0);
        this.codeStyle.set('barcode_qr');
        break;

      case '2col_roll':
        this.templateName.set('ม้วนคู่ 2 คอลัมน์ (2 คอลัมน์ - 35x25 mm)');
        this.columnsCount.set(2);
        this.rowsCount.set(null);
        this.isContinuousRoll.set(true);
        this.paperWidthMm.set(74);
        this.paperHeightMm.set(null);
        this.labelWidthMm.set(35);
        this.labelHeightMm.set(25);
        this.horizontalGapMm.set(4);
        this.verticalGapMm.set(2);
        this.marginLeftMm.set(0);
        this.marginRightMm.set(0);
        this.marginTopMm.set(0);
        this.marginBottomMm.set(0);
        this.codeStyle.set('barcode_only');
        break;

      case '3col_roll':
        this.templateName.set('ม้วน 3 คอลัมน์ (3 คอลัมน์ - 25x15 mm)');
        this.columnsCount.set(3);
        this.rowsCount.set(null);
        this.isContinuousRoll.set(true);
        this.paperWidthMm.set(80);
        this.paperHeightMm.set(null);
        this.labelWidthMm.set(25);
        this.labelHeightMm.set(15);
        this.horizontalGapMm.set(2);
        this.verticalGapMm.set(2);
        this.marginLeftMm.set(0.5);
        this.marginRightMm.set(0.5);
        this.marginTopMm.set(0);
        this.marginBottomMm.set(0);
        this.codeStyle.set('qr_only');
        break;

      case 'a4_3x8':
        this.templateName.set('กระดาษ A4 สติ๊กเกอร์ (3 คอลัมน์ x 8 แถว = 24 ดวง)');
        this.columnsCount.set(3);
        this.rowsCount.set(8);
        this.isContinuousRoll.set(false);
        this.paperWidthMm.set(210);
        this.paperHeightMm.set(297);
        this.labelWidthMm.set(70);
        this.labelHeightMm.set(36);
        this.horizontalGapMm.set(0);
        this.verticalGapMm.set(0);
        this.marginLeftMm.set(0);
        this.marginRightMm.set(0);
        this.marginTopMm.set(4.5);
        this.marginBottomMm.set(4.5);
        this.codeStyle.set('barcode_qr');
        break;
    }
  }

  // Template CRUD Operations
  saveTemplate(): void {
    if (this.isWidthExceeded()) {
      this.errorMessage.set('ความกว้างรวมของสติ๊กเกอร์เกินขนาดหน้ากว้างกระดาษ กรุณาปรับลดขนาดหรือระยะขอบ');
      return;
    }

    const payload: CreateStickerTemplateRequest = {
      name: this.templateName().trim(),
      description: this.templateDescription().trim() || undefined,
      isDefault: this.isDefault(),
      isContinuousRoll: this.isContinuousRoll(),
      paperWidthMm: this.paperWidthMm(),
      paperHeightMm: this.isContinuousRoll() ? undefined : (this.paperHeightMm() ?? undefined),
      labelWidthMm: this.labelWidthMm(),
      labelHeightMm: this.labelHeightMm(),
      columnsCount: this.columnsCount(),
      rowsCount: this.isContinuousRoll() ? undefined : (this.rowsCount() ?? undefined),
      horizontalGapMm: this.horizontalGapMm(),
      verticalGapMm: this.verticalGapMm(),
      marginTopMm: this.marginTopMm(),
      marginBottomMm: this.marginBottomMm(),
      marginLeftMm: this.marginLeftMm(),
      marginRightMm: this.marginRightMm(),
      showStoreLogo: this.showStoreLogo(),
      storeLogoUrl: this.storeLogoUrl() || undefined,
      monochromeLogo: this.monochromeLogo(),
      showStoreName: this.showStoreName(),
      storeName: this.storeName() || undefined,
      showProductTitle: this.showProductTitle(),
      showConditionGrade: this.showConditionGrade(),
      showSerialNumber: this.showSerialNumber(),
      showSku: this.showSku(),
      showPrice: this.showPrice(),
      showShutterCount: this.showShutterCount(),
      showFooterText: this.showFooterText(),
      footerText: this.footerText() || undefined,
      codeStyle: this.codeStyle(),
      encodeValue: this.encodeValue(),
      barcodeHeightMm: this.barcodeHeightMm(),
      barcodeWidthRatio: this.barcodeWidthRatio(),
      qrSizeMm: this.qrSizeMm(),
      showBarcodeText: this.showBarcodeText()
    };

    const id = this.currentTemplateId();
    if (id) {
      this.templateService.update(id, payload).subscribe({
        next: updated => {
          this.applyTemplate(updated);
          this.statusMessage.set(`บันทึกรูปแบบ "${updated.name}" เรียบร้อยแล้ว`);
          setTimeout(() => this.statusMessage.set(null), 3000);
        },
        error: err => this.errorMessage.set(err.error?.message || 'เกิดข้อผิดพลาดในการบันทึก')
      });
    } else {
      this.templateService.create(payload).subscribe({
        next: created => {
          this.applyTemplate(created);
          this.statusMessage.set(`สร้างรูปแบบ "${created.name}" เรียบร้อยแล้ว`);
          setTimeout(() => this.statusMessage.set(null), 3000);
        },
        error: err => this.errorMessage.set(err.error?.message || 'เกิดข้อผิดพลาดในการสร้าง')
      });
    }
  }

  createNewTemplate(): void {
    this.currentTemplateId.set(null);
    this.templateName.set(`รูปแบบใหม่ (${new Date().toLocaleTimeString('th-TH', { hour: '2-digit', minute: '2-digit' })})`);
    this.isDefault.set(false);
    this.statusMessage.set('กรุณาปรับแต่งการตั้งค่า แล้วกดปุ่ม "บันทึกเทมเพลต"');
  }

  duplicateTemplate(): void {
    const id = this.currentTemplateId();
    if (!id) return;

    this.templateService.duplicate(id).subscribe({
      next: dup => {
        this.applyTemplate(dup);
        this.statusMessage.set(`คัดลอกเป็น "${dup.name}" สำเร็จ`);
        setTimeout(() => this.statusMessage.set(null), 3000);
      },
      error: err => this.errorMessage.set(err.error?.message || 'ไม่สามารถคัดลอกได้')
    });
  }

  setAsDefaultTemplate(): void {
    const id = this.currentTemplateId();
    if (!id) return;

    this.templateService.setDefault(id).subscribe({
      next: () => {
        this.isDefault.set(true);
        this.statusMessage.set('ตั้งเป็นรูปแบบเริ่มต้นของร้านเรียบร้อยแล้ว');
        setTimeout(() => this.statusMessage.set(null), 3000);
      },
      error: err => this.errorMessage.set(err.error?.message || 'ไม่สามารถตั้งเป็นค่าเริ่มต้นได้')
    });
  }

  deleteTemplate(): void {
    const id = this.currentTemplateId();
    if (!id) return;

    if (!confirm(`คุณต้องการลบรูปแบบ "${this.templateName()}" ใช่หรือไม่?`)) return;

    this.templateService.delete(id).subscribe({
      next: () => {
        this.statusMessage.set('ลบรูปแบบเรียบร้อยแล้ว');
        setTimeout(() => this.statusMessage.set(null), 3000);
      },
      error: err => this.errorMessage.set(err.error?.message || 'ไม่สามารถลบได้')
    });
  }

  resetToSystemDefaults(): void {
    if (!confirm('คุณต้องการรีเซ็ตรูปแบบสติ๊กเกอร์กลับเป็นค่ามาตรฐานเริ่มต้นทั้ง 4 รูปแบบของระบบใช่หรือไม่?')) return;

    this.templateService.resetDefaults().subscribe({
      next: () => {
        this.statusMessage.set('รีเซ็ตเป็นค่ามาตรฐานระบบเรียบร้อยแล้ว');
        setTimeout(() => this.statusMessage.set(null), 3000);
      },
      error: err => this.errorMessage.set(err.error?.message || 'เกิดข้อผิดพลาดในการรีเซ็ต')
    });
  }

  nextItem(): void {
    if (this.activeItemIndex() < this.allItems().length - 1) {
      this.activeItemIndex.update(i => i + 1);
    }
  }

  prevItem(): void {
    if (this.activeItemIndex() > 0) {
      this.activeItemIndex.update(i => i - 1);
    }
  }

  private clearMessages(): void {
    this.statusMessage.set(null);
    this.errorMessage.set(null);
  }

  // Calibration test sheet generation & print
  async printCalibrationSheet(): Promise<void> {
    const cols = Math.max(1, this.columnsCount());
    const labelW = this.labelWidthMm();
    const labelH = this.labelHeightMm();
    const paperW = this.paperWidthMm();
    const isRoll = this.isContinuousRoll();
    const paperH = isRoll ? (labelH + this.verticalGapMm() + this.marginTopMm() + this.marginBottomMm() + 10) : (this.paperHeightMm() ?? 297);
    const horizGap = this.horizontalGapMm();
    const vertGap = this.verticalGapMm();
    const mTop = this.marginTopMm();
    const mLeft = this.marginLeftMm();

    let gridCellsHtml = '';
    const sampleRows = isRoll ? 2 : (this.rowsCount() ?? 4);

    for (let r = 1; r <= sampleRows; r++) {
      for (let c = 1; c <= cols; c++) {
        gridCellsHtml += `
          <div class="calib-label-box" style="width: ${labelW}mm; height: ${labelH}mm;">
            <div class="calib-box-header">R${r} C${c} (${labelW}x${labelH}mm)</div>
            <div class="calib-crosshair"></div>
            <div class="calib-dim-info">Margin L: ${mLeft}mm | T: ${mTop}mm</div>
            <div class="calib-ruler-h"></div>
            <div class="calib-ruler-v"></div>
          </div>
        `;
      }
    }

    const printHtml = `
      <!DOCTYPE html>
      <html>
      <head>
        <meta charset="utf-8">
        <title>Calibration Test Sheet — MUE2MUEDEE</title>
        <style>
          @page {
            size: ${paperW}mm ${paperH}mm;
            margin: 0;
          }
          * { box-sizing: border-box; margin: 0; padding: 0; }
          body {
            width: ${paperW}mm;
            min-height: ${paperH}mm;
            background: #fff;
            color: #000;
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
            -webkit-print-color-adjust: exact;
            print-color-adjust: exact;
          }
          .calib-page-container {
            width: ${paperW}mm;
            padding-top: ${mTop}mm;
            padding-left: ${mLeft}mm;
          }
          .calib-header {
            font-size: 8pt;
            font-weight: bold;
            margin-bottom: 2mm;
            padding-left: 1mm;
          }
          .calib-grid {
            display: grid;
            grid-template-columns: repeat(${cols}, ${labelW}mm);
            gap: ${vertGap}mm ${horizGap}mm;
          }
          .calib-label-box {
            border: 1px dashed #000;
            position: relative;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            font-size: 6pt;
            background: #fafafa;
          }
          .calib-box-header {
            font-weight: bold;
            font-size: 7pt;
          }
          .calib-crosshair {
            width: 15mm;
            height: 15mm;
            position: relative;
            margin: 1mm 0;
          }
          .calib-crosshair::before {
            content: '';
            position: absolute;
            left: 0; right: 0; top: 50%;
            height: 1px;
            background: #000;
          }
          .calib-crosshair::after {
            content: '';
            position: absolute;
            top: 0; bottom: 0; left: 50%;
            width: 1px;
            background: #000;
          }
          .calib-dim-info {
            font-size: 5pt;
            color: #333;
          }
        </style>
      </head>
      <body>
        <div class="calib-page-container">
          <div class="calib-header">📏 MUE2MUEDEE Calibration Test Sheet — Paper: ${paperW}mm | Cols: ${cols} | Sticker: ${labelW}x${labelH}mm</div>
          <div class="calib-grid">
            ${gridCellsHtml}
          </div>
        </div>
      </body>
      </html>
    `;

    this.executePrintFrame(printHtml);
  }

  // Multi-column and multi-item printing engine
  async print(): Promise<void> {
    const itemsList = this.allItems();
    if (itemsList.length === 0) return;

    const copies = Math.max(1, this.printCopies());
    const cols = Math.max(1, this.columnsCount());
    const labelW = this.labelWidthMm();
    const labelH = this.labelHeightMm();
    const paperW = this.paperWidthMm();
    const isRoll = this.isContinuousRoll();
    const paperH = isRoll ? (labelH + this.verticalGapMm()) : (this.paperHeightMm() ?? 297);
    const horizGap = this.horizontalGapMm();
    const vertGap = this.verticalGapMm();
    const mTop = this.marginTopMm();
    const mBottom = this.marginBottomMm();
    const mLeft = this.marginLeftMm();
    const mRight = this.marginRightMm();

    // Flatten all sticker items based on copies
    const flattenedItems: InventoryItem[] = [];
    for (const it of itemsList) {
      for (let c = 0; c < copies; c++) {
        flattenedItems.push(it);
      }
    }

    // Generate HTML for each sticker
    const stickerHtmls: string[] = [];
    for (const it of flattenedItems) {
      const singleHtml = await this.generateSingleStickerHtml(it, labelW, labelH);
      stickerHtmls.push(singleHtml);
    }

    // Group into pages / rows
    let bodyPagesHtml = '';

    if (isRoll) {
      // Continuous Roll: group into rows of `cols` stickers
      for (let i = 0; i < stickerHtmls.length; i += cols) {
        const rowItems = stickerHtmls.slice(i, i + cols);
        const rowInnerHtml = rowItems.join('');

        bodyPagesHtml += `
          <div class="sticker-roll-row">
            <div class="sticker-grid-row" style="grid-template-columns: repeat(${cols}, ${labelW}mm); gap: ${vertGap}mm ${horizGap}mm;">
              ${rowInnerHtml}
            </div>
          </div>
        `;
      }
    } else {
      // Sheet Mode (e.g. A4): group into pages of `cols * rowsCount`
      const stickersPerPage = (this.rowsCount() ?? 8) * cols;
      for (let i = 0; i < stickerHtmls.length; i += stickersPerPage) {
        const pageItems = stickerHtmls.slice(i, i + stickersPerPage);
        const pageInnerHtml = pageItems.join('');

        bodyPagesHtml += `
          <div class="sticker-sheet-page">
            <div class="sticker-grid-sheet" style="grid-template-columns: repeat(${cols}, ${labelW}mm); gap: ${vertGap}mm ${horizGap}mm;">
              ${pageInnerHtml}
            </div>
          </div>
        `;
      }
    }

    const fullPrintDoc = `
      <!DOCTYPE html>
      <html>
      <head>
        <meta charset="utf-8">
        <title>Print Stickers — MUE2MUEDEE</title>
        <style>
          @page {
            size: ${paperW}mm ${isRoll ? `${labelH + vertGap}mm` : `${paperH}mm`};
            margin: 0;
          }
          * {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
          }
          body {
            background: #ffffff;
            color: #000000;
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
            -webkit-print-color-adjust: exact;
            print-color-adjust: exact;
          }
          .sticker-roll-row {
            width: ${paperW}mm;
            padding-top: ${mTop}mm;
            padding-left: ${mLeft}mm;
            padding-right: ${mRight}mm;
            padding-bottom: ${mBottom}mm;
            page-break-after: always;
            break-after: page;
          }
          .sticker-grid-row {
            display: grid;
            width: fit-content;
          }
          .sticker-sheet-page {
            width: ${paperW}mm;
            height: ${paperH}mm;
            padding-top: ${mTop}mm;
            padding-bottom: ${mBottom}mm;
            padding-left: ${mLeft}mm;
            padding-right: ${mRight}mm;
            page-break-after: always;
            break-after: page;
            overflow: hidden;
          }
          .sticker-grid-sheet {
            display: grid;
            width: fit-content;
          }
          .sticker-card {
            width: ${labelW}mm;
            height: ${labelH}mm;
            overflow: hidden;
            background: #ffffff;
            border: 1px solid #000000;
            padding: 1.2mm 1.5mm;
            display: flex;
            flex-direction: column;
            justify-content: space-between;
          }
          .sticker-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 1mm;
            border-bottom: 0.5px solid #000000;
            padding-bottom: 0.8mm;
            margin-bottom: 0.8mm;
          }
          .store-brand-left {
            display: flex;
            align-items: center;
            gap: 1mm;
            min-width: 0;
            flex: 1;
          }
          .store-logo-img {
            width: 4.5mm;
            height: 4.5mm;
            object-fit: contain;
          }
          .monochrome-logo {
            filter: grayscale(100%) contrast(200%);
          }
          .store-name-text {
            font-size: 6.5pt;
            font-weight: 800;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            line-height: 1;
          }
          .badge-grade {
            border: 1px solid #000000;
            font-size: 6pt;
            font-weight: 900;
            padding: 0.2mm 1mm;
            line-height: 1;
            white-space: nowrap;
          }
          .sticker-body {
            flex: 1;
            display: flex;
            flex-direction: column;
            justify-content: center;
            gap: 0.4mm;
            min-height: 0;
          }
          .product-title {
            font-size: 7.5pt;
            font-weight: 800;
            line-height: 1.15;
            white-space: normal;
            display: -webkit-box;
            -webkit-line-clamp: 2;
            -webkit-box-orient: vertical;
            overflow: hidden;
          }
          .meta-row {
            display: flex;
            align-items: center;
            justify-content: space-between;
            font-size: 5.5pt;
            line-height: 1.1;
          }
          .sku-text {
            font-family: monospace;
            font-weight: 700;
          }
          .serial-text {
            font-family: monospace;
            font-weight: 600;
          }
          .price-text {
            font-size: 8.5pt;
            font-weight: 900;
            line-height: 1;
          }
          .sticker-footer {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 1.5mm;
            margin-top: 0.6mm;
            padding-top: 0.6mm;
            border-top: 0.5px solid #000000;
          }
          .barcode-container {
            flex: 1;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            min-width: 0;
            overflow: hidden;
          }
          .barcode-container svg {
            width: 100% !important;
            height: auto !important;
            max-height: ${this.barcodeHeightMm()}mm !important;
          }
          .barcode-number {
            font-family: monospace;
            font-size: 5pt;
            font-weight: bold;
            line-height: 1;
            margin-top: 0.2mm;
          }
          .qr-container {
            flex-shrink: 0;
            display: flex;
            align-items: center;
            justify-content: center;
          }
          .qr-container svg {
            width: ${this.qrSizeMm()}mm !important;
            height: ${this.qrSizeMm()}mm !important;
          }
          .footer-note {
            font-size: 4.5pt;
            text-align: center;
            color: #333333;
            line-height: 1;
            margin-top: 0.4mm;
          }
        </style>
      </head>
      <body>
        ${bodyPagesHtml}
      </body>
      </html>
    `;

    this.executePrintFrame(fullPrintDoc);
  }

  private async generateSingleStickerHtml(item: InventoryItem, width: number, height: number): Promise<string> {
    const encodeStr = this.encodeValue() === 'serial' ? (item.serialNumber || item.sku) : item.sku;

    // Generate Barcode SVG
    let barcodeHtml = '';
    if (this.codeStyle() !== 'qr_only') {
      try {
        const svgNode = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
        JsBarcode(svgNode, encodeStr, {
          format: 'CODE128',
          width: this.barcodeWidthRatio(),
          height: Math.max(15, this.barcodeHeightMm() * 3.5),
          displayValue: false,
          margin: 0,
          background: 'transparent',
          lineColor: '#000000'
        });
        const svgXml = new XMLSerializer().serializeToString(svgNode);
        barcodeHtml = `
          <div class="barcode-container">
            ${svgXml}
            ${this.showBarcodeText() ? `<span class="barcode-number">${encodeStr}</span>` : ''}
          </div>
        `;
      } catch (err) {
        barcodeHtml = `<div class="barcode-container"><span style="font-size:5pt;">*${encodeStr}*</span></div>`;
      }
    }

    // Generate QR Code SVG
    let qrHtml = '';
    if (this.codeStyle() !== 'barcode_only') {
      try {
        const qrSvgString = await QRCode.toString(encodeStr, {
          type: 'svg',
          margin: 0,
          errorCorrectionLevel: 'M',
          color: { dark: '#000000', light: '#ffffff00' }
        });
        qrHtml = `<div class="qr-container">${qrSvgString}</div>`;
      } catch (err) {
        qrHtml = '';
      }
    }

    // Store Logo
    const logoHtml = this.showStoreLogo() && this.storeLogoUrl()
      ? `<img src="${this.storeLogoUrl()}" alt="Logo" class="store-logo-img ${this.monochromeLogo() ? 'monochrome-logo' : ''}" />`
      : '';

    const priceFormatted = Number(item.sellingPrice || 0).toLocaleString('th-TH', { minimumFractionDigits: 0, maximumFractionDigits: 0 });

    return `
      <div class="sticker-card">
        <div class="sticker-header">
          <div class="store-brand-left">
            ${logoHtml}
            ${this.showStoreName() ? `<span class="store-name-text">${this.storeName() || 'MUE2MUEDEE'}</span>` : ''}
          </div>
          ${this.showConditionGrade() ? `<span class="badge-grade">เกรด ${item.conditionGradeCode}</span>` : ''}
        </div>

        <div class="sticker-body">
          ${this.showProductTitle() ? `<div class="product-title">${item.brandName} ${item.productModelName}</div>` : ''}
          
          <div class="meta-row">
            ${this.showSku() ? `<span class="sku-text">SKU: ${item.sku}</span>` : '<span></span>'}
            ${this.showSerialNumber() && item.serialNumber ? `<span class="serial-text">S/N: ${item.serialNumber}</span>` : '<span></span>'}
          </div>

          <div class="meta-row" style="margin-top: 0.3mm;">
            ${this.showShutterCount() && item.shutterCount ? `<span style="font-size: 5pt; color: #444;">SC: ${item.shutterCount.toLocaleString()}</span>` : '<span></span>'}
            ${this.showPrice() ? `<span class="price-text">฿${priceFormatted}</span>` : '<span></span>'}
          </div>
        </div>

        <div class="sticker-footer">
          ${barcodeHtml}
          ${qrHtml}
        </div>

        ${this.showFooterText() && this.footerText() ? `<div class="footer-note">${this.footerText()}</div>` : ''}
      </div>
    `;
  }

  private executePrintFrame(htmlContent: string): void {
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

    const doc = iframe.contentWindow?.document;
    if (!doc) {
      alert('ไม่สามารถเปิดระบบพิมพ์ของเบราว์เซอร์ได้');
      return;
    }

    doc.open();
    doc.write(htmlContent);
    doc.close();

    setTimeout(() => {
      iframe.contentWindow?.focus();
      iframe.contentWindow?.print();
    }, 450);
  }
}
