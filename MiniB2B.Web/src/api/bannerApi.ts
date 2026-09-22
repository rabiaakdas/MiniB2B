import { apiClient } from './apiClient';
import type { Banner } from '../types/shop';

export const bannerApi = {
  async getActive(): Promise<Banner[]> {
    const response = await apiClient.get<Banner[]>('/api/banners');
    return response.data;
  },
};
