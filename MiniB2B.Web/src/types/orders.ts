export type OrderStatus = 'Pending' | 'Approved' | 'Rejected';

export interface OrderItem {
  productId: number | null;
  productCode: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface OrderListItem {
  id: number;
  orderNumber: string;
  orderDate: string;
  status: OrderStatus;
  totalAmount: number;
}

export interface OrderDetail extends OrderListItem {
  items: OrderItem[];
}

export interface CreateOrderResponse {
  orderId: number;
  orderNumber: string;
  orderDate: string;
  status: OrderStatus;
  totalAmount: number;
}

export interface AdminOrderListItem extends OrderListItem {
  userId: number;
  userFullName: string;
  userEmail: string;
}

export interface AdminOrderDetail extends AdminOrderListItem {
  items: OrderItem[];
}

export interface UpdateOrderStatusRequest {
  status: 'Approved' | 'Rejected';
}
