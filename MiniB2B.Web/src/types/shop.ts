export type GridValue = string | number | boolean | null;

export type GridRenderType =
  | 'Text'
  | 'Image'
  | 'Currency'
  | 'StockStatus'
  | 'QuantityInput'
  | 'AddToCart';

export type GridAlignment = 'Left' | 'Center' | 'Right';
export type StockStatus = 'Available' | 'Critical' | 'OutOfStock';

export interface Banner {
  id: number;
  title: string;
  subtitle: string | null;
  imagePath: string;
  displayOrder: number;
  isActive: boolean;
  startDate: string | null;
  endDate: string | null;
}

export interface ProductGridColumn {
  id: number;
  fieldName: string;
  headerText: string;
  displayOrder: number;
  renderType: GridRenderType;
  width: number | null;
  alignment: GridAlignment;
  isVisibleDesktop: boolean;
  isVisibleTablet: boolean;
  isVisibleMobile: boolean;
}

export interface DynamicProductRow {
  productId: number;
  values: Record<string, GridValue>;
}

export interface ProductGridResponse {
  columns: ProductGridColumn[];
  items: DynamicProductRow[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ProductDetail {
  id: number;
  productCode: string;
  name: string;
  description: string | null;
  brand: string | null;
  manufacturerCode: string | null;
  specialCode1: string | null;
  specialCode2: string | null;
  imagePath: string | null;
  stockQuantity: number;
  criticalStockLevel: number;
  stockStatus: StockStatus;
  price: number;
  categoryId: number;
  categoryName: string;
}

export interface CartItem {
  id: number;
  productId: number;
  productCode: string;
  productName: string;
  imagePath: string | null;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
  stockQuantity: number;
  stockStatus: StockStatus;
  isAvailable: boolean;
}

export interface Cart {
  items: CartItem[];
  totalAmount: number;
  totalItemCount: number;
}

export interface AddCartItemRequest {
  productId: number;
  quantity: number;
}

export interface UpdateCartItemRequest {
  quantity: number;
}
