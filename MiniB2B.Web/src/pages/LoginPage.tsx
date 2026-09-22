import { useState, type FormEvent } from 'react';
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { getApiErrorMessage } from '../utils/apiError';

interface LocationState {
  from?: {
    pathname?: string;
  };
}

export function LoginPage() {
  const { login, isAuthenticated, user } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (isAuthenticated && user) {
    return <Navigate to={user.role === 'Admin' ? '/admin' : '/'} replace />;
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError('');
    setIsSubmitting(true);

    try {
      const currentUser = await login({ email: email.trim(), password });
      const state = location.state as LocationState | null;
      const fallbackPath = currentUser.role === 'Admin' ? '/admin' : '/';
      navigate(state?.from?.pathname ?? fallbackPath, { replace: true });
    } catch (requestError) {
      setError(getApiErrorMessage(requestError));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className="form-card" onSubmit={handleSubmit}>
      <div>
        <p className="eyebrow">Giriş</p>
        <h2>Hesabınıza giriş yapın</h2>
      </div>
      {error && <p className="form-error" role="alert">{error}</p>}
      <label htmlFor="email">Email</label>
      <input
        id="email"
        name="email"
        type="email"
        autoComplete="email"
        value={email}
        onChange={(event) => setEmail(event.target.value)}
        required
      />
      <label htmlFor="password">Şifre</label>
      <input
        id="password"
        name="password"
        type="password"
        autoComplete="current-password"
        value={password}
        onChange={(event) => setPassword(event.target.value)}
        required
      />
      <button type="submit" className="primary-button" disabled={isSubmitting}>
        {isSubmitting ? 'İşleniyor...' : 'Giriş Yap'}
      </button>
      <p className="form-footer">
        Hesabınız yok mu? <Link to="/register">Kayıt olun</Link>
      </p>
    </form>
  );
}
