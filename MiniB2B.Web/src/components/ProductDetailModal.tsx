import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { cartApi } from '../api/cartApi';
import { useAuth } from '../auth/AuthContext';
import type { ProductDetail } from '../types/shop';
import { getApiErrorMessage } from '../utils/apiError';
import { formatCurrency, stockStatusLabel } from '../utils/format';
import { MediaImage } from './MediaImage';

interface ProductDetailModalProps {
  product: ProductDetail | null;
  isLoading: boolean;
  error: string | null;
  onClose: () => void;
}

function ProductDetailPurchase({ product }: { product: ProductDetail }) {
  const { isAuthenticated } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [quantity, setQuantity] = useState(1);
  const [isAdding, setIsAdding] = useState(false);
  const [cartMessage, setCartMessage] = useState<string | null>(null);
  const [cartError, setCartError] = useState<string | null>(null);

  function handleQuantityChange(value: number) {
    const safeQuantity = Number.isFinite(value) ? Math.max(1, Math.floor(value)) : 1;
    setQuantity(safeQuantity);
  }

  async function handleAddToCart(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (product.stockStatus === 'OutOfStock') {
      return;
    }

    if (!isAuthenticated) {
      navigate('/login', { state: { from: location } });
      return;
    }

    setIsAdding(true);
    setCartMessage(null);
    setCartError(null);

    try {
      await cartApi.addItem({ productId: product.id, quantity });
      setCartMessage('Ürün sepete eklendi.');
      setQuantity(1);
    } catch (addError) {
      setCartError(getApiErrorMessage(addError));
    } finally {
      setIsAdding(false);
    }
  }

  return (
    <form className="detail-purchase" onSubmit={handleAddToCart}>
      <label htmlFor="detail-quantity">Adet</label>
      <div className="detail-purchase-row">
        <input
          id="detail-quantity"
          className="quantity-input"
          type="number"
          min={1}
          step={1}
          value={quantity}
          disabled={product.stockStatus === 'OutOfStock' || isAdding}
          onChange={(event) => handleQuantityChange(Number(event.target.value))}
        />
        <button type="submit" className="primary-button" disabled={product.stockStatus === 'OutOfStock' || isAdding}>
          {isAdding ? 'Ekleniyor...' : 'Sepete Ekle'}
        </button>
      </div>
      {product.stockStatus === 'OutOfStock' && <p className="muted">Stokta yok.</p>}
      {cartMessage && <p className="success-message">{cartMessage}</p>}
      {cartError && <p className="form-error">{cartError}</p>}
    </form>
  );
}

export function ProductDetailModal({ product, isLoading, error, onClose }: ProductDetailModalProps) {
  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        onClose();
      }
    }

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [onClose]);

  return (
    <div className="modal-backdrop" onMouseDown={onClose}>
      <section
        className="modal-panel"
        role="dialog"
        aria-modal="true"
        aria-labelledby="product-detail-title"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <button type="button" className="modal-close" onClick={onClose} aria-label="Detayı kapat">
          ×
        </button>

        {isLoading && <p className="muted">Ürün detayı yükleniyor...</p>}
        {error && <p className="form-error">{error}</p>}
        {product && (
          <div className="product-detail">
            <MediaImage path={product.imagePath} alt={product.name} className="detail-image" />
            <div className="detail-content">
              <p className="eyebrow">{product.productCode}</p>
              <h2 id="product-detail-title">{product.name}</h2>
              <p className="detail-price">{formatCurrency(product.price)}</p>
              <span className={`stock-badge stock-${product.stockStatus.toLowerCase()}`}>
                {stockStatusLabel(product.stockStatus)}
              </span>
              <dl className="detail-list">
                <div>
                  <dt>Açıklama</dt>
                  <dd>{product.description || '-'}</dd>
                </div>
                <div>
                  <dt>Marka</dt>
                  <dd>{product.brand || '-'}</dd>
                </div>
                <div>
                  <dt>Üretici Kodu</dt>
                  <dd>{product.manufacturerCode || '-'}</dd>
                </div>
                <div>
                  <dt>Özel Kod 1</dt>
                  <dd>{product.specialCode1 || '-'}</dd>
                </div>
                <div>
                  <dt>Özel Kod 2</dt>
                  <dd>{product.specialCode2 || '-'}</dd>
                </div>
                <div>
                  <dt>Stok</dt>
                  <dd>{product.stockQuantity}</dd>
                </div>
                <div>
                  <dt>Kategori</dt>
                  <dd>{product.categoryName}</dd>
                </div>
              </dl>

              <ProductDetailPurchase key={product.id} product={product} />
            </div>
          </div>
        )}
      </section>
    </div>
  );
}
