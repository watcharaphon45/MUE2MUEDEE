export interface RecentSale {
  orderId: string;
  saleNumber: string;
  customerName: string;
  itemCount: number;
  netAmount: number;
  grossProfit?: number | null;
  saleDate: string;
  status: string;
}

export interface RecentPurchase {
  purchaseOrderId: string;
  poNumber: string;
  customerName: string;
  itemCount: number;
  totalAgreedPrice: number;
  purchaseDate: string;
  status: string;
}

export interface DashboardMetrics {
  availableItems: number;
  pendingInspection: number;
  repairCount: number;
  reservedCount: number;
  totalActiveStockCount: number;
  soldThisMonthCount: number;
  salesTodayAmount: number;
  salesTodayCount: number;
  salesThisMonthAmount: number;
  salesThisMonthCount: number;
  averageOrderValue: number;
  stockAgingOver60DaysCount: number;
  stockAgingOver90DaysCount: number;
  stockAgingOver120DaysCount: number;

  // Financial metrics (Visible only to Owner/Admin)
  isFinancialVisible: boolean;
  inventoryCostValue?: number | null;
  inventorySalesValue?: number | null;
  grossProfitThisMonth?: number | null;
  averageProfitMarginThisMonth?: number | null;
  deadCapitalCost?: number | null;

  recentSales: RecentSale[];
  recentPurchases: RecentPurchase[];
}

export interface StockAgingSummary {
  bucketKey: string;
  label: string;
  statusLevel: 'fresh' | 'normal' | 'warning' | 'danger' | 'critical';
  colorCode: string;
  count: number;
  totalCost: number;
  totalSalesValue: number;
  discountPercentage: number;
}
