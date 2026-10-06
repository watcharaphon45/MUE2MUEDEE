import { StoreRole } from '../../core/auth/auth.models';

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

export interface AddUserRequest {
  email: string;
  fullName: string;
  phoneNumber?: string;
  temporaryPassword: string;
  role: string;
}

export interface UpdateStoreRequest {
  name: string;
  logoUrl?: string;
  phone?: string;
  email?: string;
  address?: string;
  taxId?: string;
  facebook?: string;
  line?: string;
  website?: string;
  primaryColor: string;
}
