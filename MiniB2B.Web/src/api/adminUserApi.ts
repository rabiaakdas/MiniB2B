import { apiClient } from './apiClient';
import type {
  AdminUserDetail,
  AdminUserListItem,
  PagedResponse,
  UpdateUserRequest,
} from '../types/admin';

export const adminUserApi = {
  async getPaged(page = 1, pageSize = 20, search?: string): Promise<PagedResponse<AdminUserListItem>> {
    const response = await apiClient.get<PagedResponse<AdminUserListItem>>('/api/admin/users', {
      params: { page, pageSize, search: search || undefined },
    });
    return response.data;
  },

  async getById(id: number): Promise<AdminUserDetail> {
    const response = await apiClient.get<AdminUserDetail>(`/api/admin/users/${id}`);
    return response.data;
  },

  async update(id: number, request: UpdateUserRequest): Promise<AdminUserDetail> {
    const response = await apiClient.put<AdminUserDetail>(`/api/admin/users/${id}`, request);
    return response.data;
  },
};
