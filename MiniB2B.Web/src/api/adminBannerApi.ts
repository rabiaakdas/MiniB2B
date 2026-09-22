import { apiClient } from './apiClient';
import type { AdminBanner, CreateBannerRequest, PagedResponse, UpdateBannerRequest } from '../types/admin';

export const adminBannerApi = {
  async getPaged(page = 1, pageSize = 20): Promise<PagedResponse<AdminBanner>> {
    const response = await apiClient.get<PagedResponse<AdminBanner>>('/api/admin/banners', {
      params: { page, pageSize },
    });
    return response.data;
  },

  async getById(id: number): Promise<AdminBanner> {
    const response = await apiClient.get<AdminBanner>(`/api/admin/banners/${id}`);
    return response.data;
  },

  async create(request: CreateBannerRequest): Promise<AdminBanner> {
    const response = await apiClient.post<AdminBanner>('/api/admin/banners', request);
    return response.data;
  },

  async update(id: number, request: UpdateBannerRequest): Promise<AdminBanner> {
    const response = await apiClient.put<AdminBanner>(`/api/admin/banners/${id}`, request);
    return response.data;
  },

  async uploadImage(id: number, file: File): Promise<AdminBanner> {
    const formData = new FormData();
    formData.append('file', file);
    const response = await apiClient.post<AdminBanner>(`/api/admin/banners/${id}/image`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await apiClient.delete(`/api/admin/banners/${id}`);
  },
};
