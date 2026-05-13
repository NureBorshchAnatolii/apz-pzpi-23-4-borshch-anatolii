import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

export function NotFound() {
  const { t } = useTranslation();
  return (
    <div className="auth-wrap">
      <div className="auth-card" style={{ textAlign: 'center' }}>
        <div className="empty-state-icon">🔍</div>
        <h1 className="auth-title">{t('errors.notFound')}</h1>
        <p className="auth-subtitle">{t('errors.notFoundDesc')}</p>
        <Link className="btn" to="/app/dashboard">{t('app.back')}</Link>
      </div>
    </div>
  );
}

export function Forbidden() {
  const { t } = useTranslation();
  return (
    <div className="auth-wrap">
      <div className="auth-card" style={{ textAlign: 'center' }}>
        <div className="empty-state-icon">🚫</div>
        <h1 className="auth-title">{t('errors.forbidden')}</h1>
        <p className="auth-subtitle">{t('errors.forbiddenDesc')}</p>
        <Link className="btn" to="/app/dashboard">{t('app.back')}</Link>
      </div>
    </div>
  );
}
