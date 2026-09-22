import { useEffect, useState } from 'react';
import { adminOrderApi } from '../api/adminOrderApi';
import { Modal } from '../components/Modal';
import { Pagination } from '../components/Pagination';
import { StatusBadge } from '../components/StatusBadge';
import { OrderItemsTable } from './OrdersPage';
import type { PagedResponse } from '../types/admin';
import type { AdminOrderDetail, AdminOrderListItem, OrderStatus } from '../types/orders';
import { getApiErrorMessage } from '../utils/apiError';
import { formatCurrency, formatDateTime } from '../utils/format';

const pageSize = 20;

export function AdminOrdersPage() {
  const [data, setData] = useState<PagedResponse<AdminOrderListItem> | null>(null);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<'' | OrderStatus>('');
  const [detail, setDetail] = useState<AdminOrderDetail | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  async function load() {
    setError(null);
    try {
      setData(await adminOrderApi.getPaged(page, pageSize, search.trim(), status));
    } catch (err) {
      setError(getApiErrorMessage(err));
    }
  }

  // oxlint-disable-next-line react/set-state-in-effect
  // oxlint-disable-next-line react-hooks/exhaustive-deps
  useEffect(() => { void load(); }, [page, status]);

  async function openDetail(id: number) {
    setError(null);
    try {
      setDetail(await adminOrderApi.getById(id));
    } catch (err) {
      setError(getApiErrorMessage(err));
    }
  }

  async function updateStatus(nextStatus: 'Approved' | 'Rejected') {
    if (!detail) return;
    if (nextStatus === 'Rejected' && !window.confirm('Siparişi reddetmek istiyor musunuz?')) return;

    try {
      const updated = await adminOrderApi.updateStatus(detail.id, { status: nextStatus });
      setDetail(updated);
      setMessage('Sipariş durumu güncellendi.');
      await load();
    } catch (err) {
      setError(getApiErrorMessage(err));
    }
  }

  return (
    <section className="page-stack">
      <div className="section-header"><div><p className="eyebrow">Admin</p><h1>Siparişler</h1></div></div>
      <form className="inline-toolbar" onSubmit={(e) => { e.preventDefault(); setPage(1); void load(); }}>
        <label className="search-field"><span>Sipariş / kullanıcı ara</span><input value={search} onChange={(e) => setSearch(e.target.value)} /></label>
        <label className="field small-field"><span>Durum</span><select value={status} onChange={(e) => { setStatus(e.target.value as '' | OrderStatus); setPage(1); }}><option value="">Tümü</option><option value="Pending">Pending</option><option value="Approved">Approved</option><option value="Rejected">Rejected</option></select></label>
        <button className="secondary-button" type="submit">Ara</button>
      </form>
      {message && <p className="success-message">{message}</p>}
      {error && <p className="form-error">{error}</p>}
      {data && data.items.length === 0 && <p className="empty-state">Sipariş bulunamadı.</p>}
      {data && data.items.length > 0 && <>
        <div className="table-scroll"><table className="data-table"><thead><tr><th>No</th><th>Kullanıcı</th><th>Email</th><th>Tarih</th><th>Toplam</th><th>Durum</th><th>Detay</th></tr></thead><tbody>
          {data.items.map((o) => <tr key={o.id}><td>{o.orderNumber}</td><td>{o.userFullName}</td><td>{o.userEmail}</td><td>{formatDateTime(o.orderDate)}</td><td>{formatCurrency(o.totalAmount)}</td><td><StatusBadge status={o.status} /></td><td><button className="secondary-button" type="button" onClick={() => void openDetail(o.id)}>Detay</button></td></tr>)}
        </tbody></table></div>
        <Pagination page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} totalPages={data.totalPages} onPageChange={setPage} />
      </>}
      {detail && <Modal title="Admin Sipariş Detayı" onClose={() => setDetail(null)} wide>
        <div className="page-stack">
          <div className="detail-list order-meta">
            <div><dt>Sipariş No</dt><dd>{detail.orderNumber}</dd></div><div><dt>Kullanıcı</dt><dd>{detail.userFullName}</dd></div><div><dt>Email</dt><dd>{detail.userEmail}</dd></div><div><dt>Tarih</dt><dd>{formatDateTime(detail.orderDate)}</dd></div><div><dt>Durum</dt><dd><StatusBadge status={detail.status} /></dd></div><div><dt>Toplam</dt><dd>{formatCurrency(detail.totalAmount)}</dd></div>
          </div>
          {detail.status === 'Pending' && <div className="row-actions"><button className="primary-button" type="button" onClick={() => void updateStatus('Approved')}>Onayla</button><button className="secondary-button danger-button" type="button" onClick={() => void updateStatus('Rejected')}>Reddet</button></div>}
          <OrderItemsTable items={detail.items} />
        </div>
      </Modal>}
    </section>
  );
}
