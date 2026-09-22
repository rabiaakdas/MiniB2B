import { Link } from 'react-router-dom';

export function ForbiddenPage() {
  return (
    <section className="page-panel compact">
      <p className="eyebrow">403</p>
      <h1>Bu sayfaya erişim yetkiniz yok.</h1>
      <p className="muted">Bu alan yalnızca yetkili kullanıcılar içindir.</p>
      <Link className="primary-link" to="/">
        Ana sayfaya dön
      </Link>
    </section>
  );
}
