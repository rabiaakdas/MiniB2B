import type { OrderStatus } from '../types/orders';

interface StatusBadgeProps {
  status: OrderStatus | string;
}

function statusText(status: OrderStatus | string): string {
  switch (status) {
    case 'Pending':
      return 'Bekliyor';
    case 'Approved':
      return 'Onaylandı';
    case 'Rejected':
      return 'Reddedildi';
    default:
      return status || '-';
  }
}

export function StatusBadge({ status }: StatusBadgeProps) {
  return <span className={`status-badge status-${status.toLowerCase()}`}>{statusText(status)}</span>;
}
