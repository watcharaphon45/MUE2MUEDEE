export interface SalesReportQuery {
  startDate?: string;
  endDate?: string;
  brandId?: string;
  categoryId?: string;
  paymentMethod?: number;
}

export interface SalesReportSummary {
  orderCount: number;
  itemsSoldCount: number;
  totalGrossSales: number;
  totalDiscount: number;
  totalNetSales: number;
  isFinancialVisible: boolean;
  totalCost?: number | null;
  totalGrossProfit?: number | null;
  averageMarginPercentage?: number | null;
}

export interface ReportGroupSummary {
  groupId: string;
  groupName: string;
  quantitySold: number;
  totalSales: number;
  totalCost?: number | null;
  grossProfit?: number | null;
  marginPercentage?: number | null;
}

export interface PaymentMethodBreakdown {
  method: number;
  methodName: string;
  transactionCount: number;
  totalAmount: number;
  percentage: number;
}

export interface DailySalesTrend {
  dateString: string;
  date: string;
  orderCount: number;
  netSales: number;
  grossProfit?: number | null;
}

export interface SalesReportOrder {
  orderId: string;
  orderNumber: string;
  orderDate: string;
  customerName: string;
  itemCount: number;
  paymentMethods: string;
  netAmount: number;
  grossProfit?: number | null;
  marginPercentage?: number | null;
  status: string;
}

export interface SalesReport {
  summary: SalesReportSummary;
  brandBreakdown: ReportGroupSummary[];
  categoryBreakdown: ReportGroupSummary[];
  paymentBreakdown: PaymentMethodBreakdown[];
  dailyTrend: DailySalesTrend[];
  orders: SalesReportOrder[];
}

export interface AgingBucketSummary {
  bucketKey: string;
  label: string;
  statusLevel: 'fresh' | 'normal' | 'warning' | 'danger' | 'critical';
  colorCode: string;
  count: number;
  totalCost: number;
  totalSalesValue: number;
  discountPercentage: number;
}

export interface AgingItemDetail {
  inventoryItemId: string;
  sku: string;
  serialNumber?: string | null;
  brandName: string;
  categoryName: string;
  modelName: string;
  daysInStock: number;
  receivedAt: string;
  conditionGrade: string;
  actualCost: number;
  currentSellingPrice: number;
  recommendedClearancePrice: number;
  agingBucket: string;
  agingDiscountPercentage: number;
  status: string;
}

export interface StockAgingReport {
  totalItemsInStock: number;
  isFinancialVisible: boolean;
  totalCostValue?: number | null;
  totalSalesValue: number;
  deadCapitalCostValue?: number | null;
  buckets: AgingBucketSummary[];
  agingItems: AgingItemDetail[];
}
