export interface CreateStoreWithOwnerPayload {
  storeName: string;
  storeCode?: string;
  phone?: string;
  email?: string;
  address?: string;
  primaryColor?: string;
  ownerFullName: string;
  ownerUsername: string;
  ownerEmail: string;
  ownerPassword: string;
  ownerPhoneNumber?: string;
}

export interface SuperAdminStoreItem {
  id: string;
  name: string;
  code: string;
  phone?: string;
  email?: string;
  address?: string;
  primaryColor: string;
  isActive: boolean;
  createdAt: string;
  ownerId?: string;
  ownerFullName?: string;
  ownerUsername?: string;
  ownerEmail?: string;
  ownerPhoneNumber?: string;
  totalUsersCount: number;
}

export interface SuperAdminStats {
  totalStores: number;
  activeStores: number;
  totalOwners: number;
  totalUsers: number;
}
