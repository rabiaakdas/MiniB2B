import { useEffect, useState } from 'react';
import { adminBannerApi } from '../api/adminBannerApi';
import { MediaImage } from '../components/MediaImage';
import { Modal } from '../components/Modal';
import { Pagination } from '../components/Pagination';
import type { AdminBanner, CreateBannerRequest, PagedResponse } from '../types/admin';
import { getApiErrorMessage } from '../utils/apiError';
import { validateImageFile } from '../utils/files';
import { formatDateTime } from '../utils/format';

const placeholderBannerPath = '/uploads/banners/9810b5e4871d46b88ab77811fa8e9ba8.png';

const emptyBanner: CreateBannerRequest = {
  title: '',
  subtitle: null,
  imagePath: placeholderBannerPath,
  displayOrder: 0,
  isActive: true,
  startDate: null,
  endDate: null,
};

export function AdminBannersPage() {
  const [data, setData] = useState<PagedResponse<AdminBanner> | null>(null);
  const [page, setPage] = useState(1);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<CreateBannerRequest>(emptyBanner);
  const [file, setFile] = useState<File | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [deletingId, setDeletingId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  async function load() {
    setError(null);
    try { setData(await adminBannerApi.getPaged(page, 20)); } catch (err) { setError(getApiErrorMessage(err)); }
  }

  // oxlint-disable-next-line react/set-state-in-effect
  // oxlint-disable-next-line react-hooks/exhaustive-deps
  useEffect(() => { void load(); }, [page]);

  async function openEdit(id: number) {
    const banner = await adminBannerApi.getById(id);
    setEditingId(id);
    setFile(null);
    setForm({
      title: banner.title,
      subtitle: banner.subtitle,
      imagePath: banner.imagePath,
      displayOrder: banner.displayOrder,
      isActive: banner.isActive,
      startDate: toDateTimeLocal(banner.startDate),
      endDate: toDateTimeLocal(banner.endDate),
    });
    setIsModalOpen(true);
  }

  function openCreate() {
    setEditingId(null);
    setFile(null);
    setForm(emptyBanner);
    setIsModalOpen(true);
  }

  function validate(): string | null {
    if (!form.title.trim()) return 'Başlık zorunludur.';
    if (form.displayOrder < 0) return 'Sıra negatif olamaz.';
    if (form.startDate && form.endDate && new Date(form.endDate) < new Date(form.startDate)) return 'Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.';
    return validateImageFile(file);
  }

  async function save() {
    const validation = validate();
    if (validation) { setError(validation); return; }
    setError(null);
    try {
      const request = { ...form, title: form.title.trim(), subtitle: form.subtitle?.trim() || null, startDate: toIsoOrNull(form.startDate), endDate: toIsoOrNull(form.endDate) };
      const saved = editingId ? await adminBannerApi.update(editingId, request) : await adminBannerApi.create(request);
      if (file) await adminBannerApi.uploadImage(saved.id, file);
      setMessage('Banner kaydedildi.');
      setIsModalOpen(false);
      await load();
    } catch (err) { setError(getApiErrorMessage(err)); }
  }

  async function deleteBanner(banner: AdminBanner) {
    const confirmed = window.confirm(`${banner.title} bannerını silmek istediğinize emin misiniz?`);

    if (!confirmed) return;

    setError(null);
    setMessage(null);
    setDeletingId(banner.id);

    try {
      await adminBannerApi.delete(banner.id);
      setMessage('Banner başarıyla silindi.');
      await load();
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setDeletingId(null);
    }
  }

  return <section className="page-stack">
    <div className="section-header"><div><p className="eyebrow">Admin</p><h1>Bannerlar</h1></div><button type="button" className="primary-button" onClick={openCreate}>Yeni Banner</button></div>
    {message && <p className="success-message">{message}</p>}{error && <p className="form-error">{error}</p>}
    {data && <><div className="table-scroll"><table className="data-table"><thead><tr><th>Görsel</th><th>Başlık</th><th>Sıra</th><th>Aktif</th><th>Başlangıç</th><th>Bitiş</th><th>İşlem</th></tr></thead><tbody>{data.items.map((b) => <tr key={b.id}><td><MediaImage path={b.imagePath} alt={b.title} className="cart-thumb" /></td><td>{b.title}</td><td>{b.displayOrder}</td><td>{b.isActive ? 'Evet' : 'Hayır'}</td><td>{formatDateTime(b.startDate)}</td><td>{formatDateTime(b.endDate)}</td><td><div className="row-actions banner-actions"><button type="button" className="secondary-button" onClick={() => void openEdit(b.id)}>Düzenle</button><button type="button" className="secondary-button danger-button" disabled={deletingId === b.id} onClick={() => void deleteBanner(b)}>{deletingId === b.id ? 'Siliniyor...' : 'Sil'}</button></div></td></tr>)}</tbody></table></div><Pagination page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} totalPages={data.totalPages} onPageChange={setPage} /></>}
    {isModalOpen && <Modal title={editingId ? 'Banner Düzenle' : 'Banner Ekle'} onClose={() => setIsModalOpen(false)} wide><div className="form-grid admin-form-grid">
      <label className="field"><span>Başlık</span><input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} /></label>
      <label className="field"><span>Alt Başlık</span><input value={form.subtitle ?? ''} onChange={(e) => setForm({ ...form, subtitle: e.target.value || null })} /></label>
      <label className="field"><span>Sıra</span><input type="number" min={0} value={form.displayOrder} onChange={(e) => setForm({ ...form, displayOrder: Number(e.target.value) })} /></label>
      <label className="field"><span>Başlangıç Tarihi</span><input type="datetime-local" value={form.startDate ?? ''} onChange={(e) => setForm({ ...form, startDate: e.target.value || null })} /></label>
      <label className="field"><span>Bitiş Tarihi</span><input type="datetime-local" value={form.endDate ?? ''} onChange={(e) => setForm({ ...form, endDate: e.target.value || null })} /></label>
      <label className="field"><span>Görsel</span><input type="file" accept="image/jpeg,image/png,image/webp" onChange={(e) => setFile(e.target.files?.[0] ?? null)} /></label>
      <label className="checkbox-field"><input type="checkbox" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} /> Aktif</label>
    </div><div className="modal-actions"><button type="button" className="primary-button" onClick={() => void save()}>Kaydet</button></div></Modal>}
  </section>;
}

function toDateTimeLocal(value: string | null): string | null {
  if (!value) return null;
  const date = new Date(value);
  const offset = date.getTimezoneOffset() * 60000;
  return new Date(date.getTime() - offset).toISOString().slice(0, 16);
}

function toIsoOrNull(value: string | null): string | null {
  return value ? new Date(value).toISOString() : null;
}
