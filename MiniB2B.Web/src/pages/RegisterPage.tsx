import { useState, type FormEvent } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { getApiErrorMessage } from '../utils/apiError';
import { emailValidationMessage, isValidEmail } from '../utils/email';
import { digitsOnly, isValidOptionalTurkishPhone, phoneValidationMessage } from '../utils/phone';

export function RegisterPage() {
  const { register, isAuthenticated, user } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    password: '',
  });
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (isAuthenticated && user) {
    return <Navigate to={user.role === 'Admin' ? '/admin' : '/'} replace />;
  }

  function updateField(field: keyof typeof form, value: string) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError('');

    if (form.password.length < 8) {
      setError('Şifre en az 8 karakter olmalıdır.');
      return;
    }

    if (!isValidEmail(form.email)) {
      setError(emailValidationMessage);
      return;
    }

    if (!isValidOptionalTurkishPhone(form.phoneNumber)) {
      setError(phoneValidationMessage);
      return;
    }

    setIsSubmitting(true);

    try {
      await register({
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        email: form.email.trim(),
        phoneNumber: form.phoneNumber.trim() || undefined,
        password: form.password,
      });
      navigate('/', { replace: true });
    } catch (requestError) {
      setError(getApiErrorMessage(requestError));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className="form-card wide" onSubmit={handleSubmit}>
      <div>
        <p className="eyebrow">Kayıt</p>
        <h2>Yeni kullanıcı hesabı oluşturun</h2>
      </div>
      {error && <p className="form-error" role="alert">{error}</p>}
      <div className="form-grid">
        <div className="field">
          <label htmlFor="firstName">Ad</label>
          <input id="firstName" value={form.firstName} onChange={(event) => updateField('firstName', event.target.value)} required maxLength={100} />
        </div>
        <div className="field">
          <label htmlFor="lastName">Soyad</label>
          <input id="lastName" value={form.lastName} onChange={(event) => updateField('lastName', event.target.value)} required maxLength={100} />
        </div>
      </div>
      <label htmlFor="registerEmail">Email</label>
      <input id="registerEmail" type="email" autoComplete="email" value={form.email} onChange={(event) => updateField('email', event.target.value)} required />
      <label htmlFor="phoneNumber">Telefon</label>
      <input
        id="phoneNumber"
        type="tel"
        inputMode="numeric"
        maxLength={11}
        pattern="05[0-9]{9}"
        value={form.phoneNumber}
        onChange={(event) => updateField('phoneNumber', digitsOnly(event.target.value))}
      />
      <label htmlFor="registerPassword">Şifre</label>
      <input id="registerPassword" type="password" autoComplete="new-password" value={form.password} onChange={(event) => updateField('password', event.target.value)} required minLength={8} />
      <button type="submit" className="primary-button" disabled={isSubmitting}>
        {isSubmitting ? 'İşleniyor...' : 'Kayıt Ol'}
      </button>
      <p className="form-footer">
        Zaten hesabınız var mı? <Link to="/login">Giriş yapın</Link>
      </p>
    </form>
  );
}
