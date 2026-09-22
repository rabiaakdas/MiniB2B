import type { Banner } from './shop';

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface CategoryLookup {
  id: number;
  name: string;
}

export interface AdminProductListItem {
  id: number;
  productCode: string;
  name: string;
  description: string | null;
  brand: string | null;
  manufacturerCode: string | null;
  specialCode1: string | null;
  specialCode2: string | null;
  stockQuantity: number;
  criticalStockLevel: number;
  price: number;
  imagePath: string | null;
  categoryId: number;
  categoryName: string;
  isActive: boolean;
}

export interface AdminProductDetail extends AdminProductListItem {
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateProductRequest {
  productCode: string;
  name: string;
  description: string | null;
  brand: string | null;
  manufacturerCode: string | null;
  specialCode1: string | null;
  specialCode2: string | null;
  stockQuantity: number;
  criticalStockLevel: number;
  price: number;
  categoryId: number;
}

export interface UpdateProductRequest extends CreateProductRequest {
  isActive: boolean;
}

export interface AdminUserListItem {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface AdminUserDetail extends AdminUserListItem {
  userName: string;
  createdAt: string;
}

export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
  isActive: boolean;
}

export interface AdminAccount {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
}

export interface UpdateAdminAccountRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
}

export interface ChangeAdminPasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface CreateAdminRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
  password: string;
  confirmPassword: string;
}

export interface CreateBannerRequest {
  title: string;
  subtitle: string | null;
  imagePath: string;
  displayOrder: number;
  isActive: boolean;
  startDate: string | null;
  endDate: string | null;
}

export type UpdateBannerRequest = CreateBannerRequest;
export type AdminBanner = Banner;
