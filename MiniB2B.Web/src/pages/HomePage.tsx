import { useCallback, useEffect, useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { bannerApi } from '../api/bannerApi';
import { cartApi } from '../api/cartApi';
import { productApi } from '../api/productApi';
import { useAuth } from '../auth/AuthContext';
import { BannerCarousel } from '../components/BannerCarousel';
import { DynamicProductGrid } from '../components/DynamicProductGrid';
import { ProductDetailModal } from '../components/ProductDetailModal';
import type { Banner, ProductDetail, ProductGridResponse } from '../types/shop';
import { getApiErrorMessage } from '../utils/apiError';

export function HomePage() {
  const { isAuthenticated } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [banners, setBanners] = useState<Banner[]>([]);
  const [grid, setGrid] = useState<ProductGridResponse | null>(null);
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
    async function loadHome() {
      setIsLoading(true);
      setError(null);

      try {
        const [bannerData, productData] = await Promise.all([
          bannerApi.getActive(),
          productApi.getGrid({ page: 1, pageSize: 5 }),
        ]);
        setBanners(bannerData);
        setGrid(productData);
      } catch (err) {
        setError(getApiErrorMessage(err));
      } finally {
        setIsLoading(false);
      }
    }

    void loadHome();
  }, []);

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

  return (
    <section className="page-stack">
      <BannerCarousel banners={banners} />

      <section className="page-panel home-intro">
        <div className="home-intro-copy">
          <p className="eyebrow">B2B Sipariş Paneli</p>
          <h1>Ürün kataloğunu inceleyin, miktarı seçin ve hızlıca siparişe hazırlayın.</h1>
          <p className="muted">Güncel stok, fiyat ve sepet işlemleri bayi sipariş akışı için tek ekranda.</p>
        </div>
        <div className="home-intro-action">
          <Link className="primary-link" to="/products">Ürün Kataloğuna Git</Link>
        </div>
      </section>

      <section className="page-stack">
        <div className="section-header">
          <div>
            <p className="eyebrow">Hızlı Sipariş</p>
            <h2>Öne çıkan ürün kataloğu</h2>
          </div>
          <Link className="text-link" to="/products">Tüm ürün kataloğu</Link>
        </div>

        {feedback && <p className={feedback.includes('eklendi') ? 'success-message' : 'form-error'}>{feedback}</p>}
        {isLoading && <p className="muted">Ana sayfa yükleniyor...</p>}
        {error && <p className="form-error">{error}</p>}
        {grid && !isLoading && !error && (
          <DynamicProductGrid
            columns={grid.columns}
            items={grid.items}
            quantities={quantities}
            addingProductIds={addingProductIds}
            onQuantityChange={handleQuantityChange}
            onAddToCart={handleAddToCart}
            onOpenDetail={openDetail}
            compact
          />
        )}
      </section>

      {detailProductId !== null && (
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
