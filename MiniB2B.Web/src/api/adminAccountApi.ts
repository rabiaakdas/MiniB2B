import { apiClient } from './apiClient';
import type {
  AdminAccount,
  ChangeAdminPasswordRequest,
  CreateAdminRequest,
  UpdateAdminAccountRequest,
} from '../types/admin';

export const adminAccountApi = {
  async getCurrent(): Promise<AdminAccount> {
    const response = await apiClient.get<AdminAccount>('/api/admin/account');
    return response.data;
  },

  async updateCurrent(request: UpdateAdminAccountRequest): Promise<AdminAccount> {
    const response = await apiClient.put<AdminAccount>('/api/admin/account', request);
    return response.data;
  },

  async changePassword(request: ChangeAdminPasswordRequest): Promise<void> {
    await apiClient.put('/api/admin/account/password', request);
  },

  async createAdmin(request: CreateAdminRequest): Promise<AdminAccount> {
    const response = await apiClient.post<AdminAccount>('/api/admin/account/admins', request);
    return response.data;
  },
};
