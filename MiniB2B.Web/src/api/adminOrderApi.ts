import { apiClient } from './apiClient';
import type { PagedResponse } from '../types/admin';
import type { AdminOrderDetail, AdminOrderListItem, OrderStatus, UpdateOrderStatusRequest } from '../types/orders';

export const adminOrderApi = {
  async getPaged(page = 1, pageSize = 20, search?: string, status?: '' | OrderStatus): Promise<PagedResponse<AdminOrderListItem>> {
    const response = await apiClient.get<PagedResponse<AdminOrderListItem>>('/api/admin/orders', {
      params: { page, pageSize, search: search || undefined, status: status || undefined },
    });
    return response.data;
  },

  async getById(id: number): Promise<AdminOrderDetail> {
    const response = await apiClient.get<AdminOrderDetail>(`/api/admin/orders/${id}`);
    return response.data;
  },

  async updateStatus(id: number, request: UpdateOrderStatusRequest): Promise<AdminOrderDetail> {
    const response = await apiClient.patch<AdminOrderDetail>(`/api/admin/orders/${id}/status`, request);
    return response.data;
  },
};
