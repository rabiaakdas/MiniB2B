import { apiClient } from './apiClient';
import type {
  AdminProductDetail,
  AdminProductListItem,
  CategoryLookup,
  CreateProductRequest,
  PagedResponse,
  UpdateProductRequest,
} from '../types/admin';

export const adminProductApi = {
  async getPaged(page = 1, pageSize = 20, search?: string): Promise<PagedResponse<AdminProductListItem>> {
    const response = await apiClient.get<PagedResponse<AdminProductListItem>>('/api/admin/products', {
      params: { page, pageSize, search: search || undefined },
    });
    return response.data;
  },

  async getById(id: number): Promise<AdminProductDetail> {
    const response = await apiClient.get<AdminProductDetail>(`/api/admin/products/${id}`);
    return response.data;
  },

  async create(request: CreateProductRequest): Promise<AdminProductDetail> {
    const response = await apiClient.post<AdminProductDetail>('/api/admin/products', request);
    return response.data;
  },

  async update(id: number, request: UpdateProductRequest): Promise<AdminProductDetail> {
    const response = await apiClient.put<AdminProductDetail>(`/api/admin/products/${id}`, request);
    return response.data;
  },

  async uploadImage(id: number, file: File): Promise<AdminProductDetail> {
    const formData = new FormData();
    formData.append('file', file);
    const response = await apiClient.post<AdminProductDetail>(`/api/admin/products/${id}/image`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await apiClient.delete(`/api/admin/products/${id}`);
  },

  async getCategories(): Promise<CategoryLookup[]> {
    const response = await apiClient.get<CategoryLookup[]>('/api/admin/categories');
    return response.data;
  },
};
