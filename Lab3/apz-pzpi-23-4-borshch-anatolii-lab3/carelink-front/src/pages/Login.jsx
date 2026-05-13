import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../context-helpers/useAuth';
import { authApi, usersApi } from '../api/endpoints';
import { LangSwitch } from '../components/LangSwitch';

export default function Login() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { login, isAuthenticated, isAdmin } = useAuth();
  const [form, setForm] = useState({ email: '', password: '' });
  const [errors, setErrors] = useState({});
  const [submitError, setSubmitError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isAuthenticated) navigate(isAdmin ? '/admin/dashboard' : '/app/dashboard', { replace: true });
  }, [isAuthenticated, isAdmin, navigate]);

  function validate() {
    const e = {};
    if (!form.email) e.email = t('validation.required');
    else if (!/^\S+@\S+\.\S+$/.test(form.email)) e.email = t('validation.invalidEmail');
    if (!form.password) e.password = t('validation.required');
    else if (form.password.length < 6) e.password = t('validation.passwordMin');
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  async function onSubmit(e) {
    e.preventDefault();
    setSubmitError('');
    if (!validate()) return;
    setLoading(true);
    try {
      const data = await authApi.login(form);
      const token = data?.token || data?.accessToken;
      if (!token) throw new Error('No token');
      const userId = data?.userId;
      localStorage.setItem('carelink_token', token);
      let profile = null;
      try { profile = userId ? await usersApi.getById(userId) : null; } catch { /* ignore */ }
      login(token, {
        userId,
        role: profile?.role || data?.role,
        roleId: profile?.roleId,
        email: profile?.email || form.email,
        firstName: profile?.firstName,
        lastName: profile?.lastName,
      });
      const isAdminRole = typeof profile?.role === 'string' && ['admin', 'administrator'].includes(profile.role.toLowerCase());
      navigate(isAdminRole ? '/admin/dashboard' : '/app/dashboard', { replace: true });
    } catch (err) {
      setSubmitError(err?.response?.data?.message || t('auth.invalidCredentials'));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-wrap">
      <div className="auth-lang"><LangSwitch /></div>
      <div className="auth-card">
        <div className="auth-logo">
          <div className="auth-logo-icon">C</div>
        </div>
        <h1 className="auth-title">{t('auth.loginTitle')}</h1>
        <p className="auth-subtitle">{t('auth.loginSubtitle')}</p>

        {submitError && <div className="auth-error">{submitError}</div>}

        <form onSubmit={onSubmit} noValidate>
          <div className="form-group">
            <label className="form-label">{t('auth.email')}</label>
            <input type="email" className="form-input" value={form.email}
              onChange={(e) => setForm({ ...form, email: e.target.value })} />
            {errors.email && <div className="form-error">{errors.email}</div>}
          </div>
          <div className="form-group">
            <label className="form-label">{t('auth.password')}</label>
            <input type="password" className="form-input" value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })} />
            {errors.password && <div className="form-error">{errors.password}</div>}
          </div>
          <button type="submit" className="btn" style={{ width: '100%', marginTop: 6 }} disabled={loading}>
            {loading ? t('app.loading') : t('auth.signIn')}
          </button>
        </form>

        <div className="auth-link">
          <Link to="/register">{t('auth.noAccount')}</Link>
        </div>
      </div>
    </div>
  );
}
