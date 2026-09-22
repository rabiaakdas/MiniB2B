import { useEffect, useState } from 'react';
import { cartApi } from '../api/cartApi';
import { orderApi } from '../api/orderApi';
import { MediaImage } from '../components/MediaImage';
import type { CreateOrderResponse } from '../types/orders';
import type { Cart } from '../types/shop';
import { formatCurrency, formatDateTime, stockStatusLabel } from '../utils/format';
import { getApiErrorMessage } from '../utils/apiError';
import { Link } from 'react-router-dom';

export function CartPage() {
  const [cart, setCart] = useState<Cart | null>(null);
  const [draftQuantities, setDraftQuantities] = useState<Record<number, string>>({});
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [updatingItemIds, setUpdatingItemIds] = useState<Set<number>>(new Set());
  const [isCreatingOrder, setIsCreatingOrder] = useState(false);
  const [createdOrder, setCreatedOrder] = useState<CreateOrderResponse | null>(null);

  async function loadCart() {
    setIsLoading(true);
    setError(null);

    try {
      const data = await cartApi.get();
      setCart(data);
      setDraftQuantities(Object.fromEntries(data.items.map((item) => [item.id, item.quantity.toString()])));
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    // oxlint-disable-next-line react/set-state-in-effect
    void loadCart();
  }, []);

  function getDraftQuantity(itemId: number, fallbackQuantity: number) {
    return draftQuantities[itemId] ?? fallbackQuantity.toString();
  }

  function parseDraftQuantity(value: string) {
    if (!/^\d+$/.test(value)) {
      return null;
    }

    const quantity = Number(value);
    return Number.isSafeInteger(quantity) && quantity >= 1 ? quantity : null;
  }

  function handleDraftChange(itemId: number, value: string) {
    if (value === '' || /^\d+$/.test(value)) {
      setDraftQuantities((current) => ({ ...current, [itemId]: value }));
    }
  }

  function normalizeDraftQuantity(itemId: number, fallbackQuantity: number) {
    const value = getDraftQuantity(itemId, fallbackQuantity);
    const quantity = parseDraftQuantity(value);

    if (quantity === null) {
      setDraftQuantities((current) => ({ ...current, [itemId]: fallbackQuantity.toString() }));
    }
  }

  function getStockLimitMessage(item: Cart['items'][number], action: 'increment' | 'update') {
    if (item.stockQuantity <= 0) {
      return `${item.productName} için stok bulunmamaktadır.`;
    }

    return action === 'increment'
      ? `${item.productName} için daha fazla stok bulunmamaktadır. Mevcut stok: ${item.stockQuantity}.`
      : `${item.productName} için yeterli stok bulunmamaktadır. Mevcut stok: ${item.stockQuantity}.`;
  }

  function stepQuantity(item: Cart['items'][number], direction: 1 | -1) {
    const currentQuantity = parseDraftQuantity(getDraftQuantity(item.id, item.quantity)) ?? item.quantity;

    if (direction === 1 && currentQuantity >= item.stockQuantity) {
      setMessage(getStockLimitMessage(item, 'increment'));
      return;
    }

    const nextQuantity = direction === 1
      ? Math.min(item.stockQuantity, currentQuantity + 1)
      : Math.max(1, currentQuantity - 1);

    setMessage(null);
    setDraftQuantities((current) => ({ ...current, [item.id]: nextQuantity.toString() }));
  }

  async function updateQuantity(itemId: number) {
    const item = cart?.items.find((cartItem) => cartItem.id === itemId);
    const quantity = parseDraftQuantity(draftQuantities[itemId] ?? '');

    if (!item || quantity === null) {
      setMessage('Lütfen geçerli bir miktar girin.');
      if (item) {
        setDraftQuantities((current) => ({ ...current, [itemId]: item.quantity.toString() }));
      }
      return;
    }

    if (quantity > item.stockQuantity) {
      setMessage(getStockLimitMessage(item, 'update'));
      setDraftQuantities((current) => ({ ...current, [itemId]: item.quantity.toString() }));
      return;
    }

    setUpdatingItemIds((current) => new Set(current).add(itemId));
    setMessage(null);

    try {
      const data = await cartApi.updateItem(itemId, { quantity });
      setCart(data);
      setDraftQuantities(Object.fromEntries(data.items.map((cartItem) => [cartItem.id, cartItem.quantity.toString()])));
      setMessage('Sepet güncellendi.');
    } catch (err) {
      setMessage(getApiErrorMessage(err));
    } finally {
      setUpdatingItemIds((current) => {
        const next = new Set(current);
        next.delete(itemId);
        return next;
      });
    }
  }

  async function removeItem(itemId: number) {
    setUpdatingItemIds((current) => new Set(current).add(itemId));
    setMessage(null);

    try {
      await cartApi.removeItem(itemId);
      await loadCart();
      setMessage('Ürün sepetten kaldırıldı.');
    } catch (err) {
      setMessage(getApiErrorMessage(err));
    } finally {
      setUpdatingItemIds((current) => {
        const next = new Set(current);
        next.delete(itemId);
        return next;
      });
    }
  }

  async function createOrder() {
    setIsCreatingOrder(true);
    setMessage(null);
    setCreatedOrder(null);

    try {
      const order = await orderApi.create();
      setCreatedOrder(order);
      setMessage('Siparişiniz oluşturuldu.');
      await loadCart();
    } catch (err) {
      setMessage(getApiErrorMessage(err));
    } finally {
      setIsCreatingOrder(false);
    }
  }

  const hasUnavailableItem = cart?.items.some((item) => !item.isAvailable) ?? false;

  return (
    <section className="page-stack">
      <div className="section-header">
        <div>
          <p className="eyebrow">Sepetim</p>
          <h1>Sepet</h1>
          {cart && <p className="muted">{cart.totalItemCount} adet ürün</p>}
        </div>
      </div>

      {isLoading && <p className="muted">Sepet yükleniyor...</p>}
      {error && <p className="form-error">{error}</p>}
      {message && <p className={message.includes('güncellendi') || message.includes('kaldırıldı') || message.includes('oluşturuldu') ? 'success-message' : 'form-error'}>{message}</p>}
      {createdOrder && (
        <div className="success-message order-success">
          <strong>{createdOrder.orderNumber}</strong>
          <span>{formatDateTime(createdOrder.orderDate)} · {formatCurrency(createdOrder.totalAmount)}</span>
          <Link className="text-link" to="/orders">Siparişlerime Git</Link>
        </div>
      )}

      {cart && cart.items.length === 0 && <p className="empty-state">Sepetiniz boş.</p>}

      {cart && cart.items.length > 0 && (
        <>
          <div className="table-scroll">
            <table className="data-table cart-table">
              <thead>
                <tr>
                  <th scope="col">Ürün</th>
                  <th scope="col">Birim Fiyat</th>
                  <th scope="col">Miktar</th>
                  <th scope="col">Toplam</th>
                  <th scope="col">İşlem</th>
                </tr>
              </thead>
              <tbody>
                {cart.items.map((item) => (
                  <tr key={item.id}>
                    <td>
                      <div className="cart-product">
                        <MediaImage path={item.imagePath} alt={item.productName} className="cart-thumb" />
                        <div>
                          <strong>{item.productName}</strong>
                          <span>{item.productCode}</span>
                          <span className={`stock-badge stock-${item.stockStatus.toLowerCase()}`}>
                            {stockStatusLabel(item.stockStatus)}
                          </span>
                          {!item.isAvailable && <span className="unavailable-note">Ürün şu anda kullanılamıyor.</span>}
                        </div>
                      </div>
                    </td>
                    <td>{formatCurrency(item.unitPrice)}</td>
                    <td>
                      <div className="quantity-stepper" aria-label={`${item.productName} miktar kontrolü`}>
                        <button
                          type="button"
                          className="quantity-stepper-button"
                          disabled={(parseDraftQuantity(getDraftQuantity(item.id, item.quantity)) ?? item.quantity) <= 1}
                          aria-label={`${item.productName} miktarını azalt`}
                          onClick={() => stepQuantity(item, -1)}
                        >
                          −
                        </button>
                        <input
                          className="quantity-input"
                          type="text"
                          inputMode="numeric"
                          pattern="[0-9]*"
                          value={getDraftQuantity(item.id, item.quantity)}
                          aria-label={`${item.productName} miktarı`}
                          onBlur={() => normalizeDraftQuantity(item.id, item.quantity)}
                          onChange={(event) => handleDraftChange(item.id, event.target.value)}
                        />
                        <button
                          type="button"
                          className="quantity-stepper-button"
                          aria-disabled={item.stockQuantity <= 0 || (parseDraftQuantity(getDraftQuantity(item.id, item.quantity)) ?? item.quantity) >= item.stockQuantity}
                          aria-label={`${item.productName} miktarını artır`}
                          onClick={() => stepQuantity(item, 1)}
                        >
                          +
                        </button>
                      </div>
                    </td>
                    <td>{formatCurrency(item.lineTotal)}</td>
                    <td>
                      <div className="row-actions">
                        <button
                          type="button"
                          className="secondary-button"
                          disabled={updatingItemIds.has(item.id)}
                          onClick={() => void updateQuantity(item.id)}
                        >
                          Güncelle
                        </button>
                        <button
                          type="button"
                          className="secondary-button danger-button"
                          disabled={updatingItemIds.has(item.id)}
                          onClick={() => void removeItem(item.id)}
                        >
                          Kaldır
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <aside className="cart-summary" aria-label="Sepet özeti">
            <span>Sepet Toplamı</span>
            <strong>{formatCurrency(cart.totalAmount)}</strong>
            {hasUnavailableItem && <p className="muted">Sepette şu anda satın alınamayacak ürünler var.</p>}
            <button
              type="button"
              className="primary-button"
              disabled={cart.items.length === 0 || isCreatingOrder}
              onClick={() => void createOrder()}
            >
              {isCreatingOrder ? 'Oluşturuluyor...' : 'Siparişi Oluştur'}
            </button>
          </aside>
        </>
      )}
    </section>
  );
}
