import { Link, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { NavLinkItem } from '../components/NavLinkItem';

export function UserLayout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate('/', { replace: true });
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <Link className="app-logo" to="/">
          MiniB2B
        </Link>
        <nav className="top-nav" aria-label="Kullanıcı menüsü">
          <NavLinkItem to="/" end>
            Ana Sayfa
          </NavLinkItem>
          <NavLinkItem to="/products">Ürün Kataloğu</NavLinkItem>
          <NavLinkItem to="/cart">Sepet</NavLinkItem>
          <NavLinkItem to="/orders">Siparişlerim</NavLinkItem>
        </nav>
        <div className="user-menu">
          {user ? (
            <>
              <span>{user.firstName} {user.lastName}</span>
              <Link className="text-link" to="/account">Hesabım</Link>
              {user.role === 'Admin' && <Link className="text-link" to="/admin">Admin</Link>}
              <button type="button" className="secondary-button" onClick={handleLogout}>
                Çıkış
              </button>
            </>
          ) : (
            <>
              <Link className="text-link" to="/login">Giriş Yap</Link>
              <Link className="secondary-button" to="/register">Kayıt Ol</Link>
            </>
          )}
        </div>
      </header>
      <main className="content-shell">
        <Outlet />
      </main>
    </div>
  );
}
