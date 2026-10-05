export enum CategoryType {
  Camera = 1,
  Lens = 2,
  Flash = 3,
  Accessory = 4,
  Other = 99
}

export enum PaymentMethod {
  Cash = 1,
  BankTransfer = 2,
  PromptPay = 3,
  CreditCard = 4,
  Other = 99
}

export enum PurchaseStatus {
  Draft = 1,
  Appraising = 2,
  Offered = 3,
  Accepted = 4,
  Rejected = 5,
  Paid = 6,
  Received = 7,
  Cancelled = 8
}

export enum InspectionItemResult {
  Pass = 1,
  MinorIssue = 2,
  Fail = 3,
  NotApplicable = 4
}

export interface PurchaseOrder {
  id: string;
  poNumber: string;
  customerId: string;
  customerName: string;
  customerPhone: string;
  customerIdCardNumber?: string;
  status: PurchaseStatus;
  statusName: string;
  totalEstimatedValue: number;
  totalMaxBuyPrice: number;
  totalOfferedPrice: number;
  totalAgreedPrice: number;
  needsApproval: boolean;
  approvedBy?: string;
  approvedAt?: string;
  notes?: string;
  purchaseDate: string;
  items: PurchaseItem[];
  payments: PurchasePayment[];
  createdAt: string;
}

export interface PurchaseItem {
  id: string;
  purchaseOrderId: string;
  productId: string;
  productModelName: string;
  productCode: string;
  brandName: string;
  categoryName: string;
  categoryType: CategoryType;
  serialNumber: string;
  conditionGradeId: string;
  conditionGradeCode: string;
  conditionGradeName: string;
  shutterCount?: number;
  accessories?: string;
  defects?: string;
  expectedSalePrice: number;
  estimatedRepairCost: number;
  estimatedCleaningCost: number;
  missingAccessoriesDeduction: number;
  maxBuyPrice: number;
  offeredPrice: number;
  agreedPrice: number;
  inventoryItemId?: string;
  generatedSku?: string;
  inspectionResults: InspectionResult[];
}

export interface InspectionResult {
  id: string;
  inspectionTemplateItemId: string;
  categoryGroup: string;
  title: string;
  isCritical: boolean;
  result: InspectionItemResult;
  resultName: string;
  notes?: string;
  photoUrl?: string;
}

export interface PurchasePayment {
  id: string;
  paymentMethod: PaymentMethod;
  paymentMethodName: string;
  amount: number;
  paidAt: string;
  referenceNumber?: string;
  proofUrl?: string;
  notes?: string;
}

export interface InspectionTemplate {
  id: string;
  categoryType: CategoryType;
  categoryTypeName: string;
  name: string;
  description?: string;
  isActive: boolean;
  items: InspectionTemplateItem[];
}

export interface InspectionTemplateItem {
  id: string;
  categoryGroup: string;
  title: string;
  description?: string;
  sortOrder: number;
  isCritical: boolean;
}

export interface CreatePurchaseOrderRequest {
  customerId: string;
  notes?: string;
  initialItems?: CreatePurchaseItemRequest[];
}

export interface CreatePurchaseItemRequest {
  productId: string;
  serialNumber: string;
  conditionGradeId: string;
  shutterCount?: number;
  accessories?: string;
  defects?: string;
  expectedSalePrice: number;
  estimatedRepairCost?: number;
  estimatedCleaningCost?: number;
  missingAccessoriesDeduction?: number;
  offeredPrice?: number;
}

export interface SaveInspectionItemRequest {
  templateItemId: string;
  result: InspectionItemResult;
  notes?: string;
  photoUrl?: string;
}

export interface MakeOfferRequest {
  totalOfferedPrice: number;
  notes?: string;
}

export interface RecordPurchasePaymentRequest {
  paymentMethod: PaymentMethod;
  amount: number;
  referenceNumber?: string;
  proofUrl?: string;
  notes?: string;
}

export interface PurchaseQueryFilter {
  search?: string;
  status?: PurchaseStatus;
  customerId?: string;
  fromDate?: string;
  toDate?: string;
  page: number;
  pageSize: number;
}
