export interface PublicStoreInfo {
  id: string;
  name: string;
  code: string;
  logoUrl?: string;
  primaryColor: string;
  address?: string;
  phone?: string;
  email?: string;
  facebook?: string;
  line?: string;
  website?: string;
  ownerName?: string;
  ownerPhone?: string;
}

export interface PublicCatalogItem {
  id: string;
  sku: string;
  modelName: string;
  brandName: string;
  categoryName: string;
  gradeName: string;
  gradeCode: string;
  sellingPrice: number;
  shutterCount?: number;
  accessories?: string;
  defects?: string;
  notes?: string;
  imageUrl?: string;
  imageUrls?: string[];
  receivedAt: string;
}

export interface CatalogFilterOption {
  id: string;
  name: string;
  code?: string;
}

export interface PublicCatalogFilterMeta {
  brands: CatalogFilterOption[];
  categories: CatalogFilterOption[];
  conditionGrades: CatalogFilterOption[];
  totalAvailableItems: number;
}

export interface PublicCatalogResponse {
  store: PublicStoreInfo;
  items: PublicCatalogItem[];
  filterMeta: PublicCatalogFilterMeta;
  totalCount: number;
  currentPage: number;
  totalPages: number;
}
