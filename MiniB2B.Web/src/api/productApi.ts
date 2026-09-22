import { apiClient } from './apiClient';
import type { ProductDetail, ProductGridResponse } from '../types/shop';

export interface ProductGridQuery {
  search?: string;
  page?: number;
  pageSize?: number;
}

export const productApi = {
  async getGrid(query: ProductGridQuery = {}): Promise<ProductGridResponse> {
    const response = await apiClient.get<ProductGridResponse>('/api/products/grid', {
      params: query,
    });
    return response.data;
  },

  async getById(id: number): Promise<ProductDetail> {
    const response = await apiClient.get<ProductDetail>(`/api/products/${id}`);
    return response.data;
  },
};
