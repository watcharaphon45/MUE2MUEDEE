export interface StickerTemplate {
  id: string;
  storeId: string;
  name: string;
  description?: string;
  isDefault: boolean;

  isContinuousRoll: boolean;
  paperWidthMm: number;
  paperHeightMm?: number;

  labelWidthMm: number;
  labelHeightMm: number;

  columnsCount: number;
  rowsCount?: number;
  horizontalGapMm: number;
  verticalGapMm: number;

  marginTopMm: number;
  marginBottomMm: number;
  marginLeftMm: number;
  marginRightMm: number;

  showStoreLogo: boolean;
  storeLogoUrl?: string;
  monochromeLogo: boolean;

  showStoreName: boolean;
  storeName?: string;

  showProductTitle: boolean;
  showConditionGrade: boolean;
  showSerialNumber: boolean;
  showSku: boolean;
  showPrice: boolean;
  showShutterCount: boolean;
  showFooterText: boolean;
  footerText?: string;

  codeStyle: 'barcode_qr' | 'barcode_only' | 'qr_only';
  encodeValue: 'serial' | 'sku';

  barcodeHeightMm: number;
  barcodeWidthRatio: number;
  qrSizeMm: number;
  showBarcodeText: boolean;

  createdAt?: string;
  updatedAt?: string;
}

export type CreateStickerTemplateRequest = Omit<StickerTemplate, 'id' | 'storeId' | 'createdAt' | 'updatedAt'>;
export type UpdateStickerTemplateRequest = CreateStickerTemplateRequest;
