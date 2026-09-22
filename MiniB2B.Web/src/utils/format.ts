import type { GridValue, StockStatus } from '../types/shop';

const tryCurrencyFormatter = new Intl.NumberFormat('tr-TR', {
  style: 'currency',
  currency: 'TRY',
});

export function formatCurrency(value: number): string {
  return tryCurrencyFormatter.format(value);
}

export function gridValueToText(value: GridValue): string {
  if (value === null) {
    return '-';
  }

  if (typeof value === 'boolean') {
    return value ? 'Evet' : 'Hayır';
  }

  return String(value);
}

export function stockStatusLabel(status: StockStatus | string | null | undefined): string {
  switch (status) {
    case 'Available':
      return 'Var';
    case 'Critical':
      return 'Kritik';
    case 'OutOfStock':
      return 'Yok';
    default:
      return '-';
  }
}

export function formatDateTime(value: string | null | undefined): string {
  if (!value) {
    return '-';
  }

  return new Intl.DateTimeFormat('tr-TR', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}
