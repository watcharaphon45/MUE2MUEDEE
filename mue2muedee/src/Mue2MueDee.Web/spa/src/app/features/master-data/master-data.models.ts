export interface Brand {
  id: string;
  name: string;
  description?: string;
  logoUrl?: string;
  isActive: boolean;
  productCount: number;
  createdAt: string;
}

export interface CreateBrandRequest {
  name: string;
  description?: string;
  logoUrl?: string;
}

export interface UpdateBrandRequest {
  name: string;
  description?: string;
  logoUrl?: string;
  isActive: boolean;
}

export interface Category {
  id: string;
  name: string;
  type: number;
  typeName: string;
  description?: string;
  isActive: boolean;
  productCount: number;
  createdAt: string;
}

export interface CreateCategoryRequest {
  name: string;
  type: number;
  description?: string;
}

export interface UpdateCategoryRequest {
  name: string;
  type: number;
  description?: string;
  isActive: boolean;
}

export interface ConditionGrade {
  id: string;
  gradeCode: string;
  gradeName: string;
  description?: string;
  sortOrder: number;
  isActive: boolean;
}

export interface CreateConditionGradeRequest {
  gradeCode: string;
  gradeName: string;
  description?: string;
  sortOrder: number;
}

export interface UpdateConditionGradeRequest {
  gradeCode: string;
  gradeName: string;
  description?: string;
  sortOrder: number;
  isActive: boolean;
}

export interface Customer {
  id: string;
  customerType: number;
  customerTypeName: string;
  name: string;
  phone: string;
  email?: string;
  idCardNumber?: string;
  address?: string;
  lineId?: string;
  facebook?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankAccountName?: string;
  notes?: string;
  isActive: boolean;
  sourcedItemCount: number;
  createdAt: string;
}

export interface CreateCustomerRequest {
  customerType: number;
  name: string;
  phone: string;
  email?: string;
  idCardNumber?: string;
  address?: string;
  lineId?: string;
  facebook?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankAccountName?: string;
  notes?: string;
}

export interface UpdateCustomerRequest {
  customerType: number;
  name: string;
  phone: string;
  email?: string;
  idCardNumber?: string;
  address?: string;
  lineId?: string;
  facebook?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankAccountName?: string;
  notes?: string;
  isActive: boolean;
}

export interface Product {
  id: string;
  brandId: string;
  brandName: string;
  categoryId: string;
  categoryName: string;
  modelName: string;
  productCode: string;
  description?: string;
  defaultSellingPrice?: number;
  imageUrl?: string;
  isActive: boolean;
  inStockCount: number;
  createdAt: string;
}

export interface CreateProductRequest {
  brandId: string;
  categoryId: string;
  modelName: string;
  productCode: string;
  description?: string;
  defaultSellingPrice?: number;
  imageUrl?: string;
}

export interface UpdateProductRequest {
  brandId: string;
  categoryId: string;
  modelName: string;
  productCode: string;
  description?: string;
  defaultSellingPrice?: number;
  imageUrl?: string;
  isActive: boolean;
}
