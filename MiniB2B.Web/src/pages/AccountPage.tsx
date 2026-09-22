import { useEffect, useState, type FormEvent } from 'react';
import { accountApi } from '../api/accountApi';
import { useAuth } from '../auth/AuthContext';
import type { ChangeUserPasswordRequest, UpdateUserAccountRequest, UserAccount } from '../types/auth';
import { getApiErrorMessage } from '../utils/apiError';
import { emailValidationMessage, isValidEmail } from '../utils/email';
import { digitsOnly, isValidOptionalTurkishPhone, phoneValidationMessage } from '../utils/phone';

const emptyPasswordForm: ChangeUserPasswordRequest = {
  currentPassword: '',
  newPassword: '',
  confirmNewPassword: '',
};

export function AccountPage() {
  const { user, updateCurrentUser } = useAuth();
  const [account, setAccount] = useState<UserAccount | null>(null);
  const [accountForm, setAccountForm] = useState<UpdateUserAccountRequest | null>(null);
  const [passwordForm, setPasswordForm] = useState<ChangeUserPasswordRequest>(emptyPasswordForm);
  const [error, setError] = useState<string | null>(null);
  const [accountMessage, setAccountMessage] = useState<string | null>(null);
  const [passwordMessage, setPasswordMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function loadAccount() {
      setError(null);

      try {
        const data = await accountApi.getCurrent();
        setAccount(data);
        setAccountForm({
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email,
          phoneNumber: data.phoneNumber,
        });
      } catch (loadError) {
        setError(getApiErrorMessage(loadError));
      } finally {
        setIsLoading(false);
      }
    }

    void loadAccount();
  }, []);

  async function handleAccountSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!accountForm) {
      return;
    }

    if (!accountForm.firstName.trim() || !accountForm.lastName.trim() || !accountForm.email.trim()) {
      setError('Ad, soyad ve e-posta zorunludur.');
      return;
    }

    if (!isValidEmail(accountForm.email)) {
      setError(emailValidationMessage);
      return;
    }

    if (!isValidOptionalTurkishPhone(accountForm.phoneNumber)) {
      setError(phoneValidationMessage);
      return;
    }

    setError(null);
    setAccountMessage(null);

    try {
      const updated = await accountApi.updateCurrent({
        ...accountForm,
        firstName: accountForm.firstName.trim(),
        lastName: accountForm.lastName.trim(),
        email: accountForm.email.trim(),
        phoneNumber: accountForm.phoneNumber?.trim() || null,
      });
      setAccount(updated);
      setAccountForm({
        firstName: updated.firstName,
        lastName: updated.lastName,
        email: updated.email,
        phoneNumber: updated.phoneNumber,
      });
      if (user) {
        updateCurrentUser({
          ...user,
          firstName: updated.firstName,
          lastName: updated.lastName,
          email: updated.email,
        });
      }
      setAccountMessage('Hesap bilgileriniz güncellendi.');
    } catch (saveError) {
      setError(getApiErrorMessage(saveError));
    }
  }

  async function handlePasswordSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setPasswordMessage(null);

    if (passwordForm.newPassword !== passwordForm.confirmNewPassword) {
      setError('Yeni şifreler eşleşmiyor.');
      return;
    }

    try {
      await accountApi.changePassword(passwordForm);
      setPasswordForm(emptyPasswordForm);
      setPasswordMessage('Şifreniz başarıyla değiştirildi.');
    } catch (passwordError) {
      setError(getApiErrorMessage(passwordError));
    }
  }

  return (
    <section className="page-stack">
      <div className="section-header">
        <div>
          <p className="eyebrow">Kullanıcı Hesabı</p>
          <h1>Hesabım</h1>
        </div>
      </div>

      {isLoading && <p className="muted">Hesap bilgileri yükleniyor...</p>}
      {error && <p className="form-error">{error}</p>}

      {account && accountForm && (
        <div className="account-sections">
          <form className="page-panel account-section" onSubmit={handleAccountSubmit}>
            <div>
              <h2>Hesap Bilgileri</h2>
              <p className="muted">Ad, soyad, e-posta ve telefon bilgilerinizi güncelleyin.</p>
            </div>
            {accountMessage && <p className="success-message">{accountMessage}</p>}
            <div className="form-grid admin-form-grid">
              <label className="field">
                <span>Ad</span>
                <input
                  value={accountForm.firstName}
                  onChange={(event) => setAccountForm({ ...accountForm, firstName: event.target.value })}
                  required
                />
              </label>
              <label className="field">
                <span>Soyad</span>
                <input
                  value={accountForm.lastName}
                  onChange={(event) => setAccountForm({ ...accountForm, lastName: event.target.value })}
                  required
                />
              </label>
              <label className="field">
                <span>E-posta</span>
                <input
                  type="email"
                  value={accountForm.email}
                  onChange={(event) => setAccountForm({ ...accountForm, email: event.target.value })}
                  required
                />
              </label>
              <label className="field">
                <span>Telefon</span>
                <input
                  type="tel"
                  inputMode="numeric"
                  maxLength={11}
                  pattern="05[0-9]{9}"
                  value={accountForm.phoneNumber ?? ''}
                  onChange={(event) => setAccountForm({ ...accountForm, phoneNumber: digitsOnly(event.target.value) || null })}
                />
              </label>
            </div>
            <div className="modal-actions">
              <button type="submit" className="primary-button">Bilgileri Güncelle</button>
            </div>
          </form>

          <form className="page-panel account-section" onSubmit={handlePasswordSubmit}>
            <div>
              <h2>Şifre Değiştir</h2>
              <p className="muted">Şifreniz mevcut şifreniz doğrulandıktan sonra Identity kurallarıyla güncellenir.</p>
            </div>
            {passwordMessage && <p className="success-message">{passwordMessage}</p>}
            <div className="form-grid admin-form-grid">
              <label className="field">
                <span>Mevcut Şifre</span>
                <input
                  type="password"
                  autoComplete="current-password"
                  value={passwordForm.currentPassword}
                  onChange={(event) => setPasswordForm({ ...passwordForm, currentPassword: event.target.value })}
                  required
                />
              </label>
              <label className="field">
                <span>Yeni Şifre</span>
                <input
                  type="password"
                  autoComplete="new-password"
                  value={passwordForm.newPassword}
                  onChange={(event) => setPasswordForm({ ...passwordForm, newPassword: event.target.value })}
                  required
                />
              </label>
              <label className="field">
                <span>Yeni Şifre Tekrar</span>
                <input
                  type="password"
                  autoComplete="new-password"
                  value={passwordForm.confirmNewPassword}
                  onChange={(event) => setPasswordForm({ ...passwordForm, confirmNewPassword: event.target.value })}
                  required
                />
              </label>
            </div>
            <div className="modal-actions">
              <button type="submit" className="primary-button">Şifreyi Değiştir</button>
            </div>
          </form>
        </div>
      )}
    </section>
  );
}
