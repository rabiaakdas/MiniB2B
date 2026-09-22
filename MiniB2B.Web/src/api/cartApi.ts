import { apiClient } from './apiClient';
import type { AddCartItemRequest, Cart, UpdateCartItemRequest } from '../types/shop';

export const cartApi = {
  async get(): Promise<Cart> {
    const response = await apiClient.get<Cart>('/api/cart');
    return response.data;
  },

  async addItem(request: AddCartItemRequest): Promise<Cart> {
    const response = await apiClient.post<Cart>('/api/cart/items', request);
    return response.data;
  },

  async updateItem(cartItemId: number, request: UpdateCartItemRequest): Promise<Cart> {
    const response = await apiClient.put<Cart>(`/api/cart/items/${cartItemId}`, request);
    return response.data;
  },

  async removeItem(cartItemId: number): Promise<void> {
    await apiClient.delete(`/api/cart/items/${cartItemId}`);
  },
};
