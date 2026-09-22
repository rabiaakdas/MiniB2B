import { useEffect, useState } from 'react';
import { adminProductApi } from '../api/adminProductApi';
import { MediaImage } from '../components/MediaImage';
import { Modal } from '../components/Modal';
import { Pagination } from '../components/Pagination';
import type { AdminProductListItem, CategoryLookup, CreateProductRequest, PagedResponse, UpdateProductRequest } from '../types/admin';
import { getApiErrorMessage } from '../utils/apiError';
import { validateImageFile } from '../utils/files';
import { formatCurrency } from '../utils/format';

const emptyProduct: UpdateProductRequest = {
  productCode: '',
  name: '',
  description: null,
  brand: null,
  manufacturerCode: null,
  specialCode1: null,
  specialCode2: null,
  stockQuantity: 0,
  criticalStockLevel: 0,
  price: 0,
  categoryId: 0,
  isActive: true,
};

export function AdminProductsPage() {
  const [data, setData] = useState<PagedResponse<AdminProductListItem> | null>(null);
  const [categories, setCategories] = useState<CategoryLookup[]>([]);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [detailProduct, setDetailProduct] = useState<AdminProductListItem | null>(null);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<UpdateProductRequest>(emptyProduct);
  const [priceInput, setPriceInput] = useState('0');
  const [file, setFile] = useState<File | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [deletingId, setDeletingId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  async function load(targetPage = page, targetSearch = search) {
    setError(null);
    try {
      const [products, categoryList] = await Promise.all([
        adminProductApi.getPaged(targetPage, 20, targetSearch.trim()),
        adminProductApi.getCategories(),
      ]);
      setData(products);
      setCategories(categoryList);
    } catch (err) {
      setError(getApiErrorMessage(err));
    }
  }

  // oxlint-disable-next-line react-hooks/exhaustive-deps
  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      void load(page, search);
    }, 400);

    return () => window.clearTimeout(timeoutId);
  }, [page, search]);

  async function openEdit(id: number) {
    setError(null);
    setFile(null);
    const product = await adminProductApi.getById(id);
    setEditingId(id);
    setForm({
      productCode: product.productCode,
      name: product.name,
      description: product.description,
      brand: product.brand,
      manufacturerCode: product.manufacturerCode,
      specialCode1: product.specialCode1,
      specialCode2: product.specialCode2,
      stockQuantity: product.stockQuantity,
      criticalStockLevel: product.criticalStockLevel,
      price: product.price,
      categoryId: product.categoryId,
      isActive: product.isActive,
    });
    setPriceInput(formatPriceInput(product.price));
    setIsModalOpen(true);
  }

  function openCreate() {
    setEditingId(null);
    setFile(null);
    setForm({ ...emptyProduct, categoryId: categories[0]?.id ?? 0, criticalStockLevel: 0, isActive: true });
    setPriceInput(formatPriceInput(emptyProduct.price));
    setIsModalOpen(true);
  }

  function validate(): string | null {
    const parsedPrice = parsePriceInput(priceInput);
    if (!form.productCode.trim()) return 'Ürün kodu zorunludur.';
    if (!form.name.trim()) return 'Ürün adı zorunludur.';
    if (parsedPrice === null) return 'Geçerli bir fiyat giriniz.';
    if (form.stockQuantity < 0 || parsedPrice < 0) return 'Stok miktarı ve fiyat negatif olamaz.';
    if (form.categoryId <= 0) return 'Ürün kaydı için aktif kategori bulunamadı.';
    return validateImageFile(file);
  }

  async function save() {
    const validation = validate();
    if (validation) {
      setError(validation);
      return;
    }

    setIsSaving(true);
    setError(null);
    try {
      const parsedPrice = parsePriceInput(priceInput);
      const clean = normalizeProduct({ ...form, price: parsedPrice ?? 0 });
      const saved = editingId
        ? await adminProductApi.update(editingId, { ...clean, isActive: form.isActive })
        : await adminProductApi.create(clean);
      if (file) {
        await adminProductApi.uploadImage(saved.id, file);
      }
      setMessage('Ürün kaydedildi.');
      setIsModalOpen(false);
      await load();
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setIsSaving(false);
    }
  }

  async function deleteProduct(id: number) {
    if (!window.confirm('Bu ürünü silmek istediğinize emin misiniz?')) {
      return;
    }

    setDeletingId(id);
    setError(null);
    setMessage(null);

    try {
      await adminProductApi.delete(id);
      setMessage('Ürün silindi.');
      await load();
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <section className="page-stack">
      <div className="section-header">
        <div><p className="eyebrow">Admin</p><h1>Ürünler</h1></div>
        <button type="button" className="primary-button" onClick={openCreate}>Yeni Ürün</button>
      </div>
      <div className="inline-toolbar">
        <label className="search-field"><span>Ürün ara</span><input value={search} onChange={(event) => { setSearch(event.target.value); setPage(1); }} /></label>
      </div>
      {message && <p className="success-message">{message}</p>}
      {error && <p className="form-error">{error}</p>}
      {data && (
        <>
          <div className="table-scroll"><table className="data-table admin-products-table"><thead><tr><th>Resim</th><th>Ürün Kodu</th><th>Ürün Adı</th><th>Marka</th><th>Stok Miktarı</th><th>Fiyat</th><th>İşlemler</th></tr></thead><tbody>
            {data.items.map((p) => <tr key={p.id}><td><MediaImage path={p.imagePath} alt={p.name} className="grid-thumb" /></td><td>{p.productCode}</td><td>{p.name}</td><td>{p.brand ?? '-'}</td><td>{p.stockQuantity}</td><td>{formatCurrency(p.price)}</td><td><div className="row-actions"><button className="secondary-button" type="button" onClick={() => setDetailProduct(p)}>Detay</button><button className="secondary-button" type="button" onClick={() => void openEdit(p.id)}>Düzenle</button><button className="secondary-button danger-button" type="button" disabled={deletingId === p.id} onClick={() => void deleteProduct(p.id)}>{deletingId === p.id ? 'Siliniyor...' : 'Sil'}</button></div></td></tr>)}
          </tbody></table></div>
          <Pagination page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} totalPages={data.totalPages} onPageChange={setPage} />
        </>
      )}
      {isModalOpen && (
        <Modal title={editingId ? 'Ürün Düzenle' : 'Ürün Ekle'} onClose={() => setIsModalOpen(false)} wide>
          <ProductForm form={form} setForm={setForm} priceInput={priceInput} setPriceInput={setPriceInput} file={file} setFile={setFile} />
          <div className="modal-actions"><button className="primary-button" disabled={isSaving} onClick={() => void save()}>{isSaving ? 'Kaydediliyor...' : 'Kaydet'}</button></div>
        </Modal>
      )}
      {detailProduct && (
        <Modal title="Ürün Detayı" onClose={() => setDetailProduct(null)} wide>
          <ProductDetailView product={detailProduct} onEdit={() => {
            const productId = detailProduct.id;
            setDetailProduct(null);
            void openEdit(productId);
          }} />
        </Modal>
      )}
    </section>
  );
}

function normalizeProduct(form: UpdateProductRequest): CreateProductRequest {
  const clean = (value: string | null) => value?.trim() || null;
  return {
    productCode: form.productCode.trim(),
    name: form.name.trim(),
    description: clean(form.description),
    brand: clean(form.brand),
    manufacturerCode: clean(form.manufacturerCode),
    specialCode1: clean(form.specialCode1),
    specialCode2: clean(form.specialCode2),
    stockQuantity: form.stockQuantity,
    criticalStockLevel: Math.max(0, form.criticalStockLevel),
    price: form.price,
    categoryId: form.categoryId,
  };
}

function parsePriceInput(value: string): number | null {
  const normalized = value.trim().replace(',', '.');

  if (!/^\d+(\.\d*)?$/.test(normalized)) {
    return null;
  }

  const parsed = Number(normalized);
  return Number.isFinite(parsed) ? parsed : null;
}

function formatPriceInput(value: number): string {
  return value.toString().replace('.', ',');
}

function ProductDetailView({ product, onEdit }: { product: AdminProductListItem; onEdit: () => void }) {
  return (
    <div className="admin-product-detail">
      <MediaImage path={product.imagePath} alt={product.name} className="detail-image" />
      <div className="detail-content">
        <p className="eyebrow">{product.productCode}</p>
        <h2>{product.name}</h2>
        <p className="detail-price">{formatCurrency(product.price)}</p>
        <dl className="detail-list">
          <div>
            <dt>Ürün Kodu</dt>
            <dd>{product.productCode}</dd>
          </div>
          <div>
            <dt>Ürün Adı</dt>
            <dd>{product.name}</dd>
          </div>
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
            <dt>Stok Miktarı</dt>
            <dd>{product.stockQuantity}</dd>
          </div>
          <div>
            <dt>Fiyat</dt>
            <dd>{formatCurrency(product.price)}</dd>
          </div>
        </dl>
        <div className="modal-actions">
          <button type="button" className="secondary-button" onClick={onEdit}>Düzenle</button>
        </div>
      </div>
    </div>
  );
}

function ProductForm({ form, setForm, priceInput, setPriceInput, file, setFile }: { form: UpdateProductRequest; setForm: (form: UpdateProductRequest) => void; priceInput: string; setPriceInput: (value: string) => void; file: File | null; setFile: (file: File | null) => void }) {
  const set = <K extends keyof UpdateProductRequest>(key: K, value: UpdateProductRequest[K]) => setForm({ ...form, [key]: value });
  return <div className="form-grid admin-form-grid">
    <label className="field"><span>Ürün Kodu</span><input value={form.productCode} onChange={(e) => set('productCode', e.target.value)} /></label>
    <label className="field"><span>Ürün Adı</span><input value={form.name} onChange={(e) => set('name', e.target.value)} /></label>
    <label className="field wide-field"><span>Açıklama</span><textarea value={form.description ?? ''} onChange={(e) => set('description', e.target.value || null)} /></label>
    <label className="field"><span>Marka</span><input value={form.brand ?? ''} onChange={(e) => set('brand', e.target.value || null)} /></label>
    <label className="field"><span>Üretici Kodu</span><input value={form.manufacturerCode ?? ''} onChange={(e) => set('manufacturerCode', e.target.value || null)} /></label>
    <label className="field"><span>Özel Kod 1</span><input value={form.specialCode1 ?? ''} onChange={(e) => set('specialCode1', e.target.value || null)} /></label>
    <label className="field"><span>Özel Kod 2</span><input value={form.specialCode2 ?? ''} onChange={(e) => set('specialCode2', e.target.value || null)} /></label>
    <label className="field"><span>Stok Miktarı</span><input type="number" min={0} value={form.stockQuantity} onChange={(e) => set('stockQuantity', Number(e.target.value))} /></label>
    <label className="field"><span>Fiyat</span><input type="text" inputMode="decimal" value={priceInput} onChange={(e) => {
      const value = e.target.value;
      setPriceInput(value);
      const parsedPrice = parsePriceInput(value);
      if (parsedPrice !== null) {
        set('price', parsedPrice);
      }
    }} /></label>
    <label className="field"><span>Ürün Görseli</span><input type="file" accept="image/jpeg,image/png,image/webp" onChange={(e) => setFile(e.target.files?.[0] ?? null)} />{file && <span className="muted">{file.name}</span>}</label>
  </div>;
}
