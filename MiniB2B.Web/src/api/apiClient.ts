import axios from 'axios';
import { notifyAuthExpired } from '../auth/authEvents';
import { tokenStorage } from '../auth/tokenStorage';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

if (!apiBaseUrl) {
  throw new Error('VITE_API_BASE_URL is not configured.');
}

export const apiClient = axios.create({
  baseURL: apiBaseUrl,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use((config) => {
  const token = tokenStorage.get();

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error?.response?.status;
    const url = error?.config?.url ?? '';
    const isAuthEndpoint = url.includes('/api/auth/login') || url.includes('/api/auth/register');

    if (status === 401 && tokenStorage.get() && !isAuthEndpoint) {
      tokenStorage.clear();
      notifyAuthExpired();
    }

    return Promise.reject(error);
  },
);
