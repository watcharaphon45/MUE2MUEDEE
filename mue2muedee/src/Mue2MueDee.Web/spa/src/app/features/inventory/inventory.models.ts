export enum InventoryStatus {
  Draft = 1,
  PendingInspection = 2,
  Available = 3,
  Reserved = 4,
  Sold = 5,
  Repair = 6,
  Returned = 7,
  Damaged = 8,
  WrittenOff = 9
}

export interface InventoryItem {
  id: string;
  sku: string;
  productId: string;
  productModelName: string;
  productCode: string;
  brandName: string;
  categoryName: string;
  productImageUrl?: string;
  serialNumber: string;
  conditionGradeId: string;
  conditionGradeCode: string;
  conditionGradeName: string;
  status: InventoryStatus;
  statusName: string;
  purchaseCost: number;
  actualCost: number;
  sellingPrice: number;
  expectedProfit: number;
  expectedMarginPercent: number;
  shutterCount?: number;
  accessories?: string;
  defects?: string;
  notes?: string;
  receivedAt: string;
  soldAt?: string;
  customerId?: string;
  customerName?: string;
  stockAgeDays: number;
  createdAt: string;
  imageUrls: string[];
}

export interface CreateInventoryItemRequest {
  productId: string;
  serialNumber: string;
  conditionGradeId: string;
  purchaseCost: number;
  actualCost?: number;
  sellingPrice: number;
  shutterCount?: number;
  accessories?: string;
  defects?: string;
  notes?: string;
  customerId?: string;
  receivedAt?: string;
  imageUrls?: string[];
}

export interface UpdateInventoryItemRequest {
  serialNumber: string;
  conditionGradeId: string;
  sellingPrice: number;
  shutterCount?: number;
  accessories?: string;
  defects?: string;
  notes?: string;
  imageUrls?: string[]; // undefined = ไม่แก้รูป
}

export interface AdjustCostRequest {
  additionalCost: number;
  costTypeReason: string;
  notes?: string;
}

export interface UpdateStatusRequest {
  newStatus: InventoryStatus;
  reason?: string;
  referenceNumber?: string;
}

export interface StockMovement {
  id: string;
  inventoryItemId: string;
  movementType: number;
  movementTypeName: string;
  quantity: number;
  referenceNumber?: string;
  notes?: string;
  createdAt: string;
  createdBy?: string;
}

export interface InventoryQueryFilter {
  search?: string;
  brandId?: string;
  categoryId?: string;
  productId?: string;
  status?: InventoryStatus;
  minPrice?: number;
  maxPrice?: number;
  page: number;
  pageSize: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export enum CostType {
  Purchase = 1,
  Repair = 2,
  Cleaning = 3,
  Shipping = 4,
  Other = 99
}

export interface InventoryCost {
  id: string;
  inventoryItemId: string;
  costType: CostType;
  costTypeName: string;
  amount: number;
  description?: string;
  incurredAt: string;
  createdAt: string;
  createdBy?: string;
}

export interface AddCostItemRequest {
  costType: CostType;
  amount: number;
  description?: string;
  incurredAt?: string;
}

export interface CostBreakdown {
  inventoryItemId: string;
  sku: string;
  modelName: string;
  purchaseCost: number;
  repairCost: number;
  cleaningCost: number;
  shippingCost: number;
  otherCost: number;
  totalActualCost: number;
  costItems: InventoryCost[];
}

export interface PriceHistory {
  id: string;
  inventoryItemId: string;
  oldPrice: number;
  newPrice: number;
  priceDifference: number;
  reason?: string;
  changedBy: string;
  changedAt: string;
}

export interface UpdatePriceRequest {
  newPrice: number;
  reason?: string;
}

export interface PricingRecommendation {
  inventoryItemId: string;
  sku: string;
  modelName: string;
  actualCost: number;
  currentSellingPrice: number;
  targetMarginPercentage: number;
  targetMarginSource: string;
  recommendedPrice: number;
  minimumPrice: number;
  maximumPrice: number;
  breakEvenPrice: number;
  estimatedGrossProfit: number;
  estimatedMarginPercentage: number;
  agingDays: number;
  agingBucket: string;
  agingColorCode: string;
  agingDiscountPercentage: number;
  pricingRationale: string;
}

