import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { usersApi } from '../../api/endpoints';
import { useAuth } from '../../context-helpers/useAuth';
import { useToast } from '../../context-helpers/useToast';
import { changeLanguage } from '../../i18n';

export default function Profile() {
  const { t, i18n } = useTranslation();
  const { user, updateUser } = useAuth();
  const toast = useToast();
  const [form, setForm] = useState({ firstName: '', lastName: '', email: '', password: '', phoneNumber: '', address: '' });
  const [loading, setLoading] = useState(!!user?.userId);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!user?.userId) return;
    usersApi.getById(user.userId)
      .then((d) => {
        if (d) setForm({
          firstName: d.firstName || '',
          lastName: d.lastName || '',
          email: d.email || user.email || '',
          phoneNumber: d.phoneNumber || '',
          address: d.address || '',
          password: '',
        });
      })
      .catch(() => toast.error(t('errors.loadFailed')))
      .finally(() => setLoading(false));
    // eslint-disable-next-line
  }, [user?.userId]);

  async function save(e) {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = {
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        phoneNumber: form.phoneNumber,
        address: form.address,
      };
      if (form.password) payload.password = form.password;
      await usersApi.update(payload);
      updateUser({ firstName: form.firstName, lastName: form.lastName, email: form.email });
      setForm({ ...form, password: '' });
      toast.success(t('profile.saved'));
    } catch {
      toast.error(t('errors.serverError'));
    } finally {
      setSaving(false);
    }
  }

  if (loading) return <div className="spinner" />;

  return (
    <div className="grid grid-cols-2">
      <div className="card">
        <div className="card-title">{t('profile.personalInfo')}</div>
        <form onSubmit={save}>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">{t('auth.firstName')}</label>
              <input className="form-input" value={form.firstName}
                onChange={(e) => setForm({ ...form, firstName: e.target.value })} />
            </div>
            <div className="form-group">
              <label className="form-label">{t('auth.lastName')}</label>
              <input className="form-input" value={form.lastName}
                onChange={(e) => setForm({ ...form, lastName: e.target.value })} />
            </div>
          </div>
          <div className="form-group">
            <label className="form-label">{t('auth.email')}</label>
            <input type="email" className="form-input" value={form.email} readOnly />
          </div>
          <div className="form-group">
            <label className="form-label">{t('auth.phone')}</label>
            <input className="form-input" value={form.phoneNumber}
              onChange={(e) => setForm({ ...form, phoneNumber: e.target.value })} />
          </div>
          <div className="form-group">
            <label className="form-label">{t('auth.address')}</label>
            <input className="form-input" value={form.address}
              onChange={(e) => setForm({ ...form, address: e.target.value })} />
          </div>
          <div className="form-group">
            <label className="form-label">{t('profile.newPassword')}</label>
            <input type="password" className="form-input" value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })} />
          </div>
          <button type="submit" className="btn" disabled={saving}>
            {saving ? t('app.loading') : t('app.save')}
          </button>
        </form>
      </div>

      <div className="card">
        <div className="card-title">{t('profile.settings')}</div>
        <div className="form-group">
          <label className="form-label">{t('profile.uiLanguage')}</label>
          <select className="form-select" value={i18n.language?.startsWith('ua') ? 'ua' : 'en'}
            onChange={(e) => changeLanguage(e.target.value)}>
            <option value="ua">Українська</option>
            <option value="en">English</option>
          </select>
        </div>
        <div className="form-group">
          <label className="form-label">{t('profile.dateFormat')}</label>
          <input className="form-input" value={i18n.language?.startsWith('ua') ? 'dd.MM.yyyy' : 'MM/dd/yyyy'} readOnly />
        </div>
      </div>
    </div>
  );
}
