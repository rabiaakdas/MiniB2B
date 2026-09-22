import { useEffect, useState, type FormEvent } from 'react';
import { adminAccountApi } from '../api/adminAccountApi';
import { useAuth } from '../auth/AuthContext';
import type {
  AdminAccount,
  ChangeAdminPasswordRequest,
  CreateAdminRequest,
  UpdateAdminAccountRequest,
} from '../types/admin';
import { getApiErrorMessage } from '../utils/apiError';
import { emailValidationMessage, isValidEmail } from '../utils/email';
import { digitsOnly, isValidOptionalTurkishPhone, phoneValidationMessage } from '../utils/phone';

const emptyPasswordForm: ChangeAdminPasswordRequest = {
  currentPassword: '',
  newPassword: '',
  confirmNewPassword: '',
};

const emptyCreateAdminForm: CreateAdminRequest = {
  firstName: '',
  lastName: '',
  email: '',
  phoneNumber: null,
  password: '',
  confirmPassword: '',
};

export function AdminAccountPage() {
  const { user, updateCurrentUser } = useAuth();
  const [account, setAccount] = useState<AdminAccount | null>(null);
  const [accountForm, setAccountForm] = useState<UpdateAdminAccountRequest | null>(null);
  const [passwordForm, setPasswordForm] = useState<ChangeAdminPasswordRequest>(emptyPasswordForm);
  const [createAdminForm, setCreateAdminForm] = useState<CreateAdminRequest>(emptyCreateAdminForm);
  const [error, setError] = useState<string | null>(null);
  const [accountMessage, setAccountMessage] = useState<string | null>(null);
  const [passwordMessage, setPasswordMessage] = useState<string | null>(null);
  const [createAdminMessage, setCreateAdminMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function loadAccount() {
      setError(null);

      try {
        const data = await adminAccountApi.getCurrent();
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
      const updated = await adminAccountApi.updateCurrent({
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
      await adminAccountApi.changePassword(passwordForm);
      setPasswordForm(emptyPasswordForm);
      setPasswordMessage('Şifreniz başarıyla değiştirildi.');
    } catch (passwordError) {
      setError(getApiErrorMessage(passwordError));
    }
  }

  async function handleCreateAdminSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setCreateAdminMessage(null);

    if (createAdminForm.password !== createAdminForm.confirmPassword) {
      setError('Şifreler eşleşmiyor.');
      return;
    }

    if (!createAdminForm.firstName.trim() || !createAdminForm.lastName.trim() || !createAdminForm.email.trim()) {
      setError('Ad, soyad ve e-posta zorunludur.');
      return;
    }

    if (!isValidEmail(createAdminForm.email)) {
      setError(emailValidationMessage);
      return;
    }

    if (!isValidOptionalTurkishPhone(createAdminForm.phoneNumber)) {
      setError(phoneValidationMessage);
      return;
    }

    try {
      const created = await adminAccountApi.createAdmin({
        ...createAdminForm,
        firstName: createAdminForm.firstName.trim(),
        lastName: createAdminForm.lastName.trim(),
        email: createAdminForm.email.trim(),
        phoneNumber: createAdminForm.phoneNumber?.trim() || null,
      });
      setCreateAdminForm(emptyCreateAdminForm);
      setCreateAdminMessage(`${created.firstName} ${created.lastName} için yönetici hesabı oluşturuldu.`);
    } catch (createError) {
      setError(getApiErrorMessage(createError));
    }
  }

  return (
    <section className="page-stack">
      <div className="section-header">
        <div>
          <p className="eyebrow">Admin</p>
          <h1>Hesabım</h1>
        </div>
      </div>

      {isLoading && <p className="muted">Hesap bilgileri yükleniyor...</p>}
      {error && <p className="form-error">{error}</p>}

      {account && accountForm && (
        <div className="account-sections">
          <form className="page-panel account-section" onSubmit={handleAccountSubmit}>
            <div>
              <h2>Hesap Bilgilerim</h2>
              <p className="muted">Kendi admin hesap bilgilerinizi güncelleyin.</p>
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
              <p className="muted">Şifreniz ASP.NET Core Identity kurallarıyla güvenli şekilde güncellenir.</p>
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

          <form className="page-panel account-section" onSubmit={handleCreateAdminSubmit}>
            <div>
              <h2>Yeni Yönetici Ekle</h2>
              <p className="muted">Yeni yönetici hesapları yalnızca mevcut admin tarafından oluşturulur.</p>
            </div>
            {createAdminMessage && <p className="success-message">{createAdminMessage}</p>}
            <div className="form-grid admin-form-grid">
              <label className="field">
                <span>Ad</span>
                <input
                  value={createAdminForm.firstName}
                  onChange={(event) => setCreateAdminForm({ ...createAdminForm, firstName: event.target.value })}
                  required
                />
              </label>
              <label className="field">
                <span>Soyad</span>
                <input
                  value={createAdminForm.lastName}
                  onChange={(event) => setCreateAdminForm({ ...createAdminForm, lastName: event.target.value })}
                  required
                />
              </label>
              <label className="field">
                <span>E-posta</span>
                <input
                  type="email"
                  value={createAdminForm.email}
                  onChange={(event) => setCreateAdminForm({ ...createAdminForm, email: event.target.value })}
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
                  value={createAdminForm.phoneNumber ?? ''}
                  onChange={(event) => setCreateAdminForm({ ...createAdminForm, phoneNumber: digitsOnly(event.target.value) || null })}
                />
              </label>
              <label className="field">
                <span>Şifre</span>
                <input
                  type="password"
                  autoComplete="new-password"
                  value={createAdminForm.password}
                  onChange={(event) => setCreateAdminForm({ ...createAdminForm, password: event.target.value })}
                  required
                />
              </label>
              <label className="field">
                <span>Şifre Tekrar</span>
                <input
                  type="password"
                  autoComplete="new-password"
                  value={createAdminForm.confirmPassword}
                  onChange={(event) => setCreateAdminForm({ ...createAdminForm, confirmPassword: event.target.value })}
                  required
                />
              </label>
            </div>
            <div className="modal-actions">
              <button type="submit" className="primary-button">Yönetici Oluştur</button>
            </div>
          </form>
        </div>
      )}
    </section>
  );
}
