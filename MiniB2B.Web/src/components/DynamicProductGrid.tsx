import type {
  DynamicProductRow,
  GridAlignment,
  GridRenderType,
  GridValue,
  ProductGridColumn,
  StockStatus,
} from '../types/shop';
import { formatCurrency, gridValueToText, stockStatusLabel } from '../utils/format';
import { MediaImage } from './MediaImage';

interface DynamicProductGridProps {
  columns: ProductGridColumn[];
  items: DynamicProductRow[];
  quantities: Record<number, number>;
  addingProductIds: Set<number>;
  onQuantityChange: (productId: number, quantity: number) => void;
  onAddToCart: (productId: number) => void;
  onOpenDetail: (productId: number) => void;
  compact?: boolean;
}

function alignmentToCss(alignment: GridAlignment): 'left' | 'center' | 'right' {
  switch (alignment) {
    case 'Center':
      return 'center';
    case 'Right':
      return 'right';
    default:
      return 'left';
  }
}

function columnVisibilityClass(column: ProductGridColumn): string {
  return [
    'product-grid-cell',
    `product-grid-cell--${column.renderType.toLowerCase()}`,
    !column.isVisibleDesktop ? 'hide-desktop' : '',
    !column.isVisibleTablet ? 'hide-tablet' : '',
    !column.isVisibleMobile ? 'hide-mobile' : '',
  ].filter(Boolean).join(' ');
}

function getGridValue(row: DynamicProductRow, fieldName: string): GridValue {
  return row.values[fieldName] ?? null;
}

function getRowStockStatus(row: DynamicProductRow, columns: ProductGridColumn[]): StockStatus | null {
  const stockColumn = columns.find((column) => column.renderType === 'StockStatus');
  if (!stockColumn) {
    return null;
  }

  const value = getGridValue(row, stockColumn.fieldName);
  if (value === 'Available' || value === 'Critical' || value === 'OutOfStock') {
    return value;
  }

  return null;
}

function gridValueToNumber(value: GridValue): number | null {
  return typeof value === 'number' ? value : null;
}

interface RenderCellArgs {
  column: ProductGridColumn;
  row: DynamicProductRow;
  value: GridValue;
  quantity: number;
  isAdding: boolean;
  isOutOfStock: boolean;
  onQuantityChange: (productId: number, quantity: number) => void;
  onAddToCart: (productId: number) => void;
  onOpenDetail: (productId: number) => void;
}

function renderCell({
  column,
  row,
  value,
  quantity,
  isAdding,
  isOutOfStock,
  onQuantityChange,
  onAddToCart,
  onOpenDetail,
}: RenderCellArgs) {
  const renderType: GridRenderType = column.renderType;

  switch (renderType) {
    case 'Image':
      return <MediaImage path={typeof value === 'string' ? value : null} alt="Ürün görseli" className="grid-thumb" />;
    case 'Currency': {
      const numericValue = gridValueToNumber(value);
      return numericValue === null ? '-' : formatCurrency(numericValue);
    }
    case 'StockStatus': {
      const status = typeof value === 'string' ? value : null;
      return <span className={`stock-badge stock-${status?.toLowerCase() ?? 'unknown'}`}>{stockStatusLabel(status)}</span>;
    }
    case 'QuantityInput':
      return (
        <input
          className="quantity-input"
          type="number"
          min={1}
          step={1}
          value={quantity}
          disabled={isOutOfStock}
          aria-label={`Ürün ${row.productId} miktarı`}
          onChange={(event) => onQuantityChange(row.productId, Number(event.target.value))}
        />
      );
    case 'AddToCart':
      return (
        <button
          type="button"
          className="primary-button grid-action"
          disabled={isAdding || isOutOfStock}
          onClick={() => onAddToCart(row.productId)}
        >
          {isAdding ? 'Ekleniyor...' : 'Sepete Ekle'}
        </button>
      );
    case 'Text':
    default:
      return (
        <button type="button" className="cell-link" onClick={() => onOpenDetail(row.productId)}>
          {gridValueToText(value)}
        </button>
      );
  }
}

export function DynamicProductGrid({
  columns,
  items,
  quantities,
  addingProductIds,
  onQuantityChange,
  onAddToCart,
  onOpenDetail,
  compact = false,
}: DynamicProductGridProps) {
  if (items.length === 0) {
    return <p className="empty-state">Ürün bulunamadı.</p>;
  }

  return (
    <div className={compact ? 'table-scroll compact-grid' : 'table-scroll'}>
      <table className="data-table product-grid">
        <thead>
          <tr>
            {columns.map((column) => (
              <th
                key={column.id}
                className={columnVisibilityClass(column)}
                data-field={column.fieldName}
                style={{
                  width: column.width ? `${column.width}px` : undefined,
                  textAlign: alignmentToCss(column.alignment),
                }}
                scope="col"
              >
                {column.headerText}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {items.map((row) => {
            const stockStatus = getRowStockStatus(row, columns);
            const isOutOfStock = stockStatus === 'OutOfStock';

            return (
              <tr key={row.productId}>
                {columns.map((column) => {
                  const value = getGridValue(row, column.fieldName);
                  return (
                    <td
                      key={column.id}
                      className={columnVisibilityClass(column)}
                      data-field={column.fieldName}
                      style={{
                        width: column.width ? `${column.width}px` : undefined,
                        textAlign: alignmentToCss(column.alignment),
                      }}
                    >
                      {renderCell({
                        column,
                        row,
                        value,
                        quantity: quantities[row.productId] ?? 1,
                        isAdding: addingProductIds.has(row.productId),
                        isOutOfStock,
                        onQuantityChange,
                        onAddToCart,
                        onOpenDetail,
                      })}
                    </td>
                  );
                })}
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
