import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export function AdminLayout() {
  const { user, logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const isDashboard = location.pathname === '/admin';

  function handleLogout() {
    logout();
    navigate('/login', { replace: true });
  }

  return (
    <div className="admin-shell">
      <header className="admin-topbar">
        <Link className="app-logo" to="/admin">
          MiniB2B Admin
        </Link>
        <div>
          <strong>{user?.firstName} {user?.lastName}</strong>
          <span>{user?.email}</span>
        </div>
        <div className="admin-actions">
          <Link className="text-link" to="/admin/account">Hesabım</Link>
          <button type="button" className="secondary-button" onClick={handleLogout}>
            Çıkış
          </button>
        </div>
      </header>
      <main className="content-shell admin-content">
        {!isDashboard && (
          <Link className="admin-back-link" to="/admin">
            ← Yönetim Paneli
          </Link>
        )}
        <Outlet />
      </main>
    </div>
  );
}
