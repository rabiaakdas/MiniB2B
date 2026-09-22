import { useEffect, useState } from 'react';
import { orderApi } from '../api/orderApi';
import { Modal } from '../components/Modal';
import { Pagination } from '../components/Pagination';
import { StatusBadge } from '../components/StatusBadge';
import type { PagedResponse } from '../types/admin';
import type { OrderDetail, OrderListItem } from '../types/orders';
import { getApiErrorMessage } from '../utils/apiError';
import { formatCurrency, formatDateTime } from '../utils/format';

const pageSize = 10;

export function OrdersPage() {
  const [orders, setOrders] = useState<PagedResponse<OrderListItem> | null>(null);
  const [page, setPage] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [detail, setDetail] = useState<OrderDetail | null>(null);
  const [detailError, setDetailError] = useState<string | null>(null);

  async function loadOrders() {
    setIsLoading(true);
    setError(null);

    try {
      setOrders(await orderApi.getPaged(page, pageSize));
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setIsLoading(false);
    }
  }

  // oxlint-disable-next-line react/set-state-in-effect
  // oxlint-disable-next-line react-hooks/exhaustive-deps
  useEffect(() => { void loadOrders(); }, [page]);

  async function openDetail(id: number) {
    setDetail(null);
    setDetailError(null);

    try {
      setDetail(await orderApi.getById(id));
    } catch (err) {
      setDetailError(getApiErrorMessage(err));
    }
  }

  return (
    <section className="page-stack">
      <div className="section-header">
        <div>
          <p className="eyebrow">Siparişlerim</p>
          <h1>Siparişler</h1>
        </div>
      </div>

      {isLoading && <p className="muted">Siparişler yükleniyor...</p>}
      {error && <p className="form-error">{error}</p>}
      {orders && orders.items.length === 0 && <p className="empty-state">Henüz siparişiniz yok.</p>}
      {orders && orders.items.length > 0 && (
        <>
          <div className="table-scroll">
            <table className="data-table">
              <thead>
                <tr>
                  <th scope="col">Sipariş No</th>
                  <th scope="col">Tarih</th>
                  <th scope="col">Toplam</th>
                  <th scope="col">Durum</th>
                  <th scope="col">Detay</th>
                </tr>
              </thead>
              <tbody>
                {orders.items.map((order) => (
                  <tr key={order.id}>
                    <td>{order.orderNumber}</td>
                    <td>{formatDateTime(order.orderDate)}</td>
                    <td>{formatCurrency(order.totalAmount)}</td>
                    <td><StatusBadge status={order.status} /></td>
                    <td>
                      <button type="button" className="secondary-button" onClick={() => void openDetail(order.id)}>
                        Detay
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <Pagination page={orders.page} pageSize={orders.pageSize} totalCount={orders.totalCount} totalPages={orders.totalPages} onPageChange={setPage} />
        </>
      )}

      {(detail || detailError) && (
        <Modal title="Sipariş Detayı" onClose={() => { setDetail(null); setDetailError(null); }} wide>
          {detailError && <p className="form-error">{detailError}</p>}
          {detail && (
            <div className="page-stack">
              <div className="detail-list order-meta">
                <div><dt>Sipariş No</dt><dd>{detail.orderNumber}</dd></div>
                <div><dt>Tarih</dt><dd>{formatDateTime(detail.orderDate)}</dd></div>
                <div><dt>Durum</dt><dd><StatusBadge status={detail.status} /></dd></div>
                <div><dt>Toplam</dt><dd>{formatCurrency(detail.totalAmount)}</dd></div>
              </div>
              <OrderItemsTable items={detail.items} />
            </div>
          )}
        </Modal>
      )}
    </section>
  );
}

export function OrderItemsTable({ items }: { items: OrderDetail['items'] }) {
  return (
    <div className="table-scroll">
      <table className="data-table">
        <thead>
          <tr>
            <th scope="col">Ürün Kodu</th>
            <th scope="col">Ürün</th>
            <th scope="col">Miktar</th>
            <th scope="col">Birim Fiyat</th>
            <th scope="col">Toplam</th>
          </tr>
        </thead>
        <tbody>
          {items.map((item, index) => (
            <tr key={`${item.productCode}-${index}`}>
              <td>{item.productCode}</td>
              <td>{item.productName}</td>
              <td>{item.quantity}</td>
              <td>{formatCurrency(item.unitPrice)}</td>
              <td>{formatCurrency(item.totalPrice)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
