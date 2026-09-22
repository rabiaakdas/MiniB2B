import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <section className="page-panel compact">
      <p className="eyebrow">404</p>
      <h1>Sayfa bulunamadı.</h1>
      <p className="muted">Aradığınız adres mevcut değil veya taşınmış olabilir.</p>
      <Link className="primary-link" to="/">
        Ana sayfaya dön
      </Link>
    </section>
  );
}
