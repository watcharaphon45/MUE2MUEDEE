export enum SaleStatus {
  Draft = 1,
  Completed = 2,
  Cancelled = 3,
  Refunded = 4
}

export enum PaymentMethod {
  Cash = 1,
  BankTransfer = 2,
  PromptPay = 3,
  CreditCard = 4,
  Other = 99
}

export interface SaleItem {
  id: string;
  saleOrderId: string;
  inventoryItemId: string;
  productId: string;
  productModelName: string;
  sku: string;
  serialNumber: string;
  conditionGradeCode: string;
  actualCost: number;
  unitPrice: number;
  discount: number;
  subTotal: number;
  grossProfit: number;
  marginPercentage: number;
  warrantyDays: number;
}

export interface SalePayment {
  id: string;
  saleOrderId: string;
  paymentMethod: PaymentMethod;
  paymentMethodName: string;
  amount: number;
  referenceNumber?: string;
  paidAt: string;
  notes?: string;
  createdBy: string;
}

export interface SaleOrder {
  id: string;
  saleNumber: string;
  customerId?: string;
  customerName?: string;
  customerPhone?: string;
  saleDate: string;
  status: SaleStatus;
  statusName: string;
  totalAmount: number;
  discountAmount: number;
  netAmount: number;
  totalCost: number;
  totalGrossProfit: number;
  totalMarginPercentage: number;
  notes?: string;
  createdBy: string;
  createdAt: string;
  items: SaleItem[];
  payments: SalePayment[];
}

export interface CreateSaleItemRequest {
  inventoryItemId: string;
  unitPrice: number;
  discount?: number;
  warrantyDays?: number;
}

export interface CreateSalePaymentRequest {
  paymentMethod: PaymentMethod;
  amount: number;
  referenceNumber?: string;
  notes?: string;
}

export interface CreateSaleOrderRequest {
  customerId?: string;
  customerName?: string;
  customerPhone?: string;
  notes?: string;
  items: CreateSaleItemRequest[];
  payments: CreateSalePaymentRequest[];
}

export interface SaleQueryFilter {
  search?: string;
  status?: SaleStatus;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface SerialTrace {
  serialNumber: string;
  inventoryItemId: string;
  sku: string;
  productModelName: string;
  brandName: string;
  categoryName: string;
  conditionGradeCode: string;
  conditionGradeName: string;
  currentStatusName: string;
  sourcedFromCustomer?: string;
  purchasePoNumber?: string;
  receivedAt?: string;
  purchaseCost: number;
  actualCost: number;
  currentSellingPrice: number;
  soldToCustomer?: string;
  saleNumber?: string;
  soldAt?: string;
  soldPrice?: number;
  grossProfit?: number;
  marginPercentage?: number;
  movements: {
    id: string;
    inventoryItemId: string;
    movementType: number;
    movementTypeName: string;
    quantity: number;
    referenceNumber?: string;
    notes?: string;
    createdAt: string;
    createdBy?: string;
  }[];
}
