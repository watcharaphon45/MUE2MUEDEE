export type StoreRole = 'Owner' | 'Admin' | 'Staff';

export interface UserStoreInfo {
  storeId: string;
  storeName: string;
  storeCode: string;
  logoUrl?: string;
  role: StoreRole;
  primaryColor: string;
}

export interface AuthUser {
  id: string;
  username: string;
  email: string;
  fullName: string;
  avatarUrl?: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: AuthUser;
  currentStoreId?: string;
  currentRole?: StoreRole;
  availableStores: UserStoreInfo[];
}

export interface StoreDetails {
  id: string;
  name: string;
  code: string;
  logoUrl?: string;
  phone?: string;
  email?: string;
  address?: string;
  taxId?: string;
  facebook?: string;
  line?: string;
  website?: string;
  primaryColor: string;
  isActive: boolean;
  createdAt: string;
}

export interface StoreUser {
  userId: string;
  username: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  avatarUrl?: string;
  role: StoreRole;
  isActive: boolean;
  joinedAt: string;
}
