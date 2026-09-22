import { apiClient } from './apiClient';
import type { PagedResponse } from '../types/admin';
import type { CreateOrderResponse, OrderDetail, OrderListItem } from '../types/orders';

export const orderApi = {
  async create(): Promise<CreateOrderResponse> {
    const response = await apiClient.post<CreateOrderResponse>('/api/orders');
    return response.data;
  },

  async getPaged(page = 1, pageSize = 20): Promise<PagedResponse<OrderListItem>> {
    const response = await apiClient.get<PagedResponse<OrderListItem>>('/api/orders', {
      params: { page, pageSize },
    });
    return response.data;
  },

  async getById(id: number): Promise<OrderDetail> {
    const response = await apiClient.get<OrderDetail>(`/api/orders/${id}`);
    return response.data;
  },
};
