import { useEffect, useState } from 'react';
import { adminUserApi } from '../api/adminUserApi';
import { Modal } from '../components/Modal';
import { Pagination } from '../components/Pagination';
import type { AdminUserDetail, AdminUserListItem, PagedResponse, UpdateUserRequest } from '../types/admin';
import { getApiErrorMessage } from '../utils/apiError';
import { emailValidationMessage, isValidEmail } from '../utils/email';
import { formatDateTime } from '../utils/format';
import { digitsOnly, isValidOptionalTurkishPhone, phoneValidationMessage } from '../utils/phone';

export function AdminUsersPage() {
  const [data, setData] = useState<PagedResponse<AdminUserListItem> | null>(null);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [viewDetail, setViewDetail] = useState<AdminUserDetail | null>(null);
  const [editDetail, setEditDetail] = useState<AdminUserDetail | null>(null);
  const [form, setForm] = useState<UpdateUserRequest | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  async function load(targetPage = page, targetSearch = search) {
    setError(null);
    try { setData(await adminUserApi.getPaged(targetPage, 20, targetSearch.trim())); } catch (err) { setError(getApiErrorMessage(err)); }
  }

  // oxlint-disable-next-line react-hooks/exhaustive-deps
  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      void load(page, search);
    }, 400);

    return () => window.clearTimeout(timeoutId);
  }, [page, search]);

  async function openView(id: number) {
    setError(null);
    setMessage(null);
    setViewDetail(await adminUserApi.getById(id));
  }

  async function openEdit(id: number) {
    setError(null);
    setMessage(null);
    const user = await adminUserApi.getById(id);
    setEditDetail(user);
    setForm({ firstName: user.firstName, lastName: user.lastName, email: user.email, phoneNumber: user.phoneNumber, isActive: user.isActive });
  }

  async function saveUser() {
    if (!editDetail || !form) return;
    if (!form.firstName.trim() || !form.lastName.trim() || !form.email.trim()) { setError('Ad, soyad ve email zorunludur.'); return; }
    if (!isValidEmail(form.email)) { setError(emailValidationMessage); return; }
    if (!isValidOptionalTurkishPhone(form.phoneNumber)) { setError(phoneValidationMessage); return; }
    try {
      const updatedUser = await adminUserApi.update(editDetail.id, { ...form, firstName: form.firstName.trim(), lastName: form.lastName.trim(), email: form.email.trim() });
      setEditDetail(updatedUser);
      setForm({ firstName: updatedUser.firstName, lastName: updatedUser.lastName, email: updatedUser.email, phoneNumber: updatedUser.phoneNumber, isActive: updatedUser.isActive });
      setMessage('Kullanıcı güncellendi.');
      await load();
    } catch (err) { setError(getApiErrorMessage(err)); }
  }

  return <section className="page-stack">
    <div className="section-header"><div><p className="eyebrow">Admin</p><h1>Kullanıcılar</h1></div></div>
    <div className="inline-toolbar"><label className="search-field"><span>Kullanıcı ara</span><input value={search} onChange={(e) => { setSearch(e.target.value); setPage(1); }} /></label></div>
    {message && <p className="success-message">{message}</p>}{error && <p className="form-error">{error}</p>}
    {data && <><div className="table-scroll"><table className="data-table"><thead><tr><th>Ad</th><th>Soyad</th><th>E-posta</th><th>Telefon</th><th>Durum</th><th>Kayıt Tarihi</th><th>İşlemler</th></tr></thead><tbody>{data.items.map((u) => <tr key={u.id}><td>{u.firstName}</td><td>{u.lastName}</td><td>{u.email}</td><td>{u.phoneNumber ?? '-'}</td><td>{u.isActive ? 'Aktif' : 'Pasif'}</td><td>{formatDateTime(u.createdAt)}</td><td><div className="row-actions"><button type="button" className="secondary-button" onClick={() => void openView(u.id)}>Görüntüle</button><button type="button" className="secondary-button" onClick={() => void openEdit(u.id)}>Düzenle</button></div></td></tr>)}</tbody></table></div><Pagination page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} totalPages={data.totalPages} onPageChange={setPage} /></>}
    {viewDetail && <Modal title="Kullanıcı Detayı" onClose={() => setViewDetail(null)} wide>
      <dl className="detail-list">
        <div><dt>Ad</dt><dd>{viewDetail.firstName}</dd></div>
        <div><dt>Soyad</dt><dd>{viewDetail.lastName}</dd></div>
        <div><dt>E-posta</dt><dd>{viewDetail.email}</dd></div>
        <div><dt>Telefon</dt><dd>{viewDetail.phoneNumber ?? '-'}</dd></div>
        <div><dt>Kullanıcı Adı / E-posta</dt><dd>{viewDetail.userName || viewDetail.email}</dd></div>
        <div><dt>Durum</dt><dd>{viewDetail.isActive ? 'Aktif' : 'Pasif'}</dd></div>
        <div><dt>Kayıt Tarihi</dt><dd>{formatDateTime(viewDetail.createdAt)}</dd></div>
      </dl>
      <div className="modal-actions"><button type="button" className="secondary-button" onClick={() => setViewDetail(null)}>Kapat</button></div>
    </Modal>}
    {editDetail && form && <Modal title="Kullanıcı Düzenle" onClose={() => setEditDetail(null)} wide><div className="form-grid admin-form-grid">
      <label className="field"><span>Ad</span><input value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} /></label>
      <label className="field"><span>Soyad</span><input value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} /></label>
      <label className="field"><span>Email</span><input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} /></label>
      <label className="field"><span>Telefon</span><input type="tel" inputMode="numeric" maxLength={11} pattern="05[0-9]{9}" value={form.phoneNumber ?? ''} onChange={(e) => setForm({ ...form, phoneNumber: digitsOnly(e.target.value) || null })} /></label>
      <label className="checkbox-field"><input type="checkbox" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} /> {form.isActive ? 'Aktif' : 'Pasif'}</label>
    </div><div className="modal-actions"><button type="button" className="primary-button" onClick={() => void saveUser()}>Kaydet</button></div>
    </Modal>}
  </section>;
}
