import { useCallback, useEffect, useMemo, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { cartApi } from '../api/cartApi';
import { productApi } from '../api/productApi';
import { useAuth } from '../auth/AuthContext';
import { DynamicProductGrid } from '../components/DynamicProductGrid';
import { Pagination } from '../components/Pagination';
import { ProductDetailModal } from '../components/ProductDetailModal';
import type { ProductDetail, ProductGridResponse } from '../types/shop';
import { getApiErrorMessage } from '../utils/apiError';

const pageSize = 20;

export function ProductsPage() {
  const { isAuthenticated } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [grid, setGrid] = useState<ProductGridResponse | null>(null);
  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [page, setPage] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [feedback, setFeedback] = useState<string | null>(null);
  const [quantities, setQuantities] = useState<Record<number, number>>({});
  const [addingProductIds, setAddingProductIds] = useState<Set<number>>(new Set());
  const [detailProduct, setDetailProduct] = useState<ProductDetail | null>(null);
  const [detailProductId, setDetailProductId] = useState<number | null>(null);
  const [isDetailLoading, setIsDetailLoading] = useState(false);
  const [detailError, setDetailError] = useState<string | null>(null);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedSearch(search.trim());
      setPage(1);
    }, 350);

    return () => window.clearTimeout(timeoutId);
  }, [search]);

  const loadGrid = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const data = await productApi.getGrid({
        search: debouncedSearch || undefined,
        page,
        pageSize,
      });
      setGrid(data);
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setIsLoading(false);
    }
  }, [debouncedSearch, page]);

  useEffect(() => {
    // oxlint-disable-next-line react/set-state-in-effect
    void loadGrid();
  }, [loadGrid]);

  const openDetail = useCallback(async (productId: number) => {
    setDetailProductId(productId);
    setDetailProduct(null);
    setDetailError(null);
    setIsDetailLoading(true);

    try {
      const product = await productApi.getById(productId);
      setDetailProduct(product);
    } catch (err) {
      setDetailError(getApiErrorMessage(err));
    } finally {
      setIsDetailLoading(false);
    }
  }, []);

  function closeDetail() {
    setDetailProductId(null);
    setDetailProduct(null);
    setDetailError(null);
  }

  function handleQuantityChange(productId: number, quantity: number) {
    const safeQuantity = Number.isFinite(quantity) ? Math.max(1, Math.floor(quantity)) : 1;
    setQuantities((current) => ({ ...current, [productId]: safeQuantity }));
  }

  async function handleAddToCart(productId: number) {
    if (!isAuthenticated) {
      navigate('/login', { state: { from: location } });
      return;
    }

    const quantity = quantities[productId] ?? 1;
    setAddingProductIds((current) => new Set(current).add(productId));
    setFeedback(null);

    try {
      await cartApi.addItem({ productId, quantity });
      setFeedback('Ürün sepete eklendi.');
    } catch (err) {
      setFeedback(getApiErrorMessage(err));
    } finally {
      setAddingProductIds((current) => {
        const next = new Set(current);
        next.delete(productId);
        return next;
      });
    }
  }

  const modalOpen = detailProductId !== null;
  const totalText = useMemo(() => {
    if (!grid) {
      return '';
    }

    return `${grid.totalCount} ürün`;
  }, [grid]);

  return (
    <section className="page-stack">
      <div className="section-header">
        <div>
          <p className="eyebrow">Katalog</p>
          <h1>Ürün Kataloğu</h1>
          {totalText && <p className="muted">{totalText}</p>}
        </div>
        <label className="search-field">
          <span>Ürün ara</span>
          <input
            type="search"
            value={search}
            placeholder="Kod, ad, marka veya açıklama"
            onChange={(event) => setSearch(event.target.value)}
          />
        </label>
      </div>

      {feedback && <p className={feedback.includes('eklendi') ? 'success-message' : 'form-error'}>{feedback}</p>}
      {isLoading && <p className="muted">Ürünler yükleniyor...</p>}
      {error && <p className="form-error">{error}</p>}
      {grid && !isLoading && !error && (
        <>
          <DynamicProductGrid
            columns={grid.columns}
            items={grid.items}
            quantities={quantities}
            addingProductIds={addingProductIds}
            onQuantityChange={handleQuantityChange}
            onAddToCart={handleAddToCart}
            onOpenDetail={openDetail}
          />
          <Pagination
            page={grid.page}
            pageSize={grid.pageSize}
            totalCount={grid.totalCount}
            totalPages={grid.totalPages}
            onPageChange={setPage}
          />
        </>
      )}

      {modalOpen && (
        <ProductDetailModal
          product={detailProduct}
          isLoading={isDetailLoading}
          error={detailError}
          onClose={closeDetail}
        />
      )}
    </section>
  );
}
