import { AxiosError } from 'axios';
import type { ApiErrorResponse } from '../types/api';

export function getApiErrorMessage(error: unknown): string {
  if (error instanceof AxiosError) {
    if (!error.response) {
      return 'Sunucuya ulaşılamadı.';
    }

    const data = error.response.data as ApiErrorResponse | undefined;

    if (data?.errors && data.errors.length > 0) {
      return data.errors.join(' ');
    }

    if (data?.detail) {
      return data.detail;
    }

    if (data?.title) {
      return data.title;
    }

    if (error.response.status === 401) {
      return 'Bu işlem için giriş yapmanız gerekiyor.';
    }
  }

  return 'Beklenmeyen bir hata oluştu.';
}
