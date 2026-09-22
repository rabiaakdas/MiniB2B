import { apiClient } from './apiClient';
import type { ChangeUserPasswordRequest, UpdateUserAccountRequest, UserAccount } from '../types/auth';

export const accountApi = {
  async getCurrent(): Promise<UserAccount> {
    const response = await apiClient.get<UserAccount>('/api/account');
    return response.data;
  },

  async updateCurrent(request: UpdateUserAccountRequest): Promise<UserAccount> {
    const response = await apiClient.put<UserAccount>('/api/account', request);
    return response.data;
  },

  async changePassword(request: ChangeUserPasswordRequest): Promise<void> {
    await apiClient.put('/api/account/password', request);
  },
};
