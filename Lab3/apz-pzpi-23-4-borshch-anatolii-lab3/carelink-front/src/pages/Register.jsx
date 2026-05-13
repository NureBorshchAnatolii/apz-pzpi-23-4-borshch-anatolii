import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { authApi, rolesApi } from '../api/endpoints';
import { LangSwitch } from '../components/LangSwitch';
import { useToast } from '../context-helpers/useToast';

export default function Register() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const toast = useToast();
  const [form, setForm] = useState({
    firstName: '', lastName: '', email: '', password: '', confirmPassword: '',
    roleId: '', phoneNumber: '', address: '', birthDate: '',
  });
  const [errors, setErrors] = useState({});
  const [submitError, setSubmitError] = useState('');
  const [loading, setLoading] = useState(false);
  const [roles, setRoles] = useState([]);

  useEffect(() => {
    rolesApi.list().then((data) => {
      setRoles(Array.isArray(data) ? data : []);
    }).catch(() => {});
  }, []);

  function validate() {
    const e = {};
    if (!form.firstName) e.firstName = t('validation.required');
    if (!form.lastName) e.lastName = t('validation.required');
    if (!form.email) e.email = t('validation.required');
    else if (!/^\S+@\S+\.\S+$/.test(form.email)) e.email = t('validation.invalidEmail');
    if (!form.password) e.password = t('validation.required');
    else if (form.password.length < 6) e.password = t('validation.passwordMin');
    if (form.confirmPassword !== form.password) e.confirmPassword = t('validation.passwordMatch');
    if (!form.roleId) e.roleId = t('validation.required');
    if (!form.phoneNumber || form.phoneNumber.length < 10) e.phoneNumber = t('validation.required');
    if (!form.address || form.address.length < 5) e.address = t('validation.required');
    if (!form.birthDate) e.birthDate = t('validation.required');
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  async function onSubmit(e) {
    e.preventDefault();
    setSubmitError('');
    if (!validate()) return;
    setLoading(true);
    try {
      await authApi.register({
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        password: form.password,
        roleId: Number(form.roleId),
        birthDate: form.birthDate,
        address: form.address,
        phoneNumber: form.phoneNumber,
      });
      toast.success(t('auth.registerSuccess'));
      navigate('/login', { replace: true });
    } catch (err) {
      setSubmitError(err?.response?.data?.message || t('errors.serverError'));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-wrap">
      <div className="auth-lang"><LangSwitch /></div>
      <div className="auth-card" style={{ maxWidth: 520 }}>
        <div className="auth-logo"><div className="auth-logo-icon">C</div></div>
        <h1 className="auth-title">{t('auth.registerTitle')}</h1>
        <p className="auth-subtitle">{t('auth.registerSubtitle')}</p>

        {submitError && <div className="auth-error">{submitError}</div>}

        <form onSubmit={onSubmit} noValidate>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">{t('auth.firstName')}</label>
              <input className="form-input" value={form.firstName}
                onChange={(e) => setForm({ ...form, firstName: e.target.value })} />
              {errors.firstName && <div className="form-error">{errors.firstName}</div>}
            </div>
            <div className="form-group">
              <label className="form-label">{t('auth.lastName')}</label>
              <input className="form-input" value={form.lastName}
                onChange={(e) => setForm({ ...form, lastName: e.target.value })} />
              {errors.lastName && <div className="form-error">{errors.lastName}</div>}
            </div>
          </div>

          <div className="form-group">
            <label className="form-label">{t('auth.email')}</label>
            <input type="email" className="form-input" value={form.email}
              onChange={(e) => setForm({ ...form, email: e.target.value })} />
            {errors.email && <div className="form-error">{errors.email}</div>}
          </div>

          <div className="form-row">
            <div className="form-group">
              <label className="form-label">{t('auth.password')}</label>
              <input type="password" className="form-input" value={form.password}
                onChange={(e) => setForm({ ...form, password: e.target.value })} />
              {errors.password && <div className="form-error">{errors.password}</div>}
            </div>
            <div className="form-group">
              <label className="form-label">{t('auth.confirmPassword')}</label>
              <input type="password" className="form-input" value={form.confirmPassword}
                onChange={(e) => setForm({ ...form, confirmPassword: e.target.value })} />
              {errors.confirmPassword && <div className="form-error">{errors.confirmPassword}</div>}
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label className="form-label">{t('auth.role')}</label>
              <select className="form-select" value={form.roleId}
                onChange={(e) => setForm({ ...form, roleId: e.target.value })}>
                <option value="">—</option>
                {roles.map((r) => (
                  <option key={r.id ?? r.typeId} value={r.id ?? r.typeId}>
                    {t(`roles.${r.name}`, { defaultValue: r.name })}
                  </option>
                ))}
              </select>
              {errors.roleId && <div className="form-error">{errors.roleId}</div>}
            </div>
            <div className="form-group">
              <label className="form-label">{t('auth.birthDate')}</label>
              <input type="date" className="form-input" value={form.birthDate}
                onChange={(e) => setForm({ ...form, birthDate: e.target.value })} />
              {errors.birthDate && <div className="form-error">{errors.birthDate}</div>}
            </div>
          </div>

          <div className="form-group">
            <label className="form-label">{t('auth.phone')}</label>
            <input className="form-input" value={form.phoneNumber}
              onChange={(e) => setForm({ ...form, phoneNumber: e.target.value })} />
            {errors.phoneNumber && <div className="form-error">{errors.phoneNumber}</div>}
          </div>

          <div className="form-group">
            <label className="form-label">{t('auth.address')}</label>
            <input className="form-input" value={form.address}
              onChange={(e) => setForm({ ...form, address: e.target.value })} />
            {errors.address && <div className="form-error">{errors.address}</div>}
          </div>

          <button type="submit" className="btn" style={{ width: '100%', marginTop: 6 }} disabled={loading}>
            {loading ? t('app.loading') : t('auth.signUp')}
          </button>
        </form>

        <div className="auth-link"><Link to="/login">{t('auth.hasAccount')}</Link></div>
      </div>
    </div>
  );
}
