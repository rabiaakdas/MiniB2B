import { Outlet } from 'react-router-dom';

export function AuthLayout() {
  return (
    <main className="auth-shell">
      <section className="auth-panel" aria-label="Kimlik doğrulama">
        <div className="brand-block">
          <span className="brand-mark">B2B</span>
          <div>
            <p className="eyebrow">MiniB2B</p>
            <h1>Bayiler için sade sipariş deneyimi</h1>
          </div>
        </div>
        <Outlet />
      </section>
    </main>
  );
}
