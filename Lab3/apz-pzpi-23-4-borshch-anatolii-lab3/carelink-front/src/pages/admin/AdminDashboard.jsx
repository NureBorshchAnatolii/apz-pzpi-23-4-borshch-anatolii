import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { adminApi, deviceTypesApi, exercisesApi, rolesApi } from '../../api/endpoints';
import { useToast } from '../../context-helpers/useToast';

export default function AdminDashboard() {
  const { t } = useTranslation();
  const toast = useToast();
  const [state, setState] = useState(null);
  const [counts, setCounts] = useState({ roles: 0, deviceTypes: 0, exercises: 0 });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.allSettled([
      adminApi.systemState(),
      rolesApi.list(),
      deviceTypesApi.list(),
      exercisesApi.list(),
    ]).then(([s, r, d, e]) => {
      if (s.status === 'fulfilled') setState(s.value);
      setCounts({
        roles: r.status === 'fulfilled' && Array.isArray(r.value) ? r.value.length : 0,
        deviceTypes: d.status === 'fulfilled' && Array.isArray(d.value) ? d.value.length : 0,
        exercises: e.status === 'fulfilled' && Array.isArray(e.value) ? e.value.length : 0,
      });
    }).catch(() => toast.error(t('errors.loadFailed')))
      .finally(() => setLoading(false));
    // eslint-disable-next-line
  }, []);

  if (loading) return <div className="spinner" />;

  return (
    <div className="grid grid-cols-3">
      <div className="card" style={{ gridColumn: 'span 2' }}>
        <div className="card-title">{t('admin.systemState')}</div>
        <div className="metric-row">
          <span>{t('admin.uptime')}</span>
          <strong>{state?.uptime || '—'}</strong>
        </div>
        <div className="metric-row">
          <span>{t('admin.activeUsers')}</span>
          <strong>{state?.activeUsers ?? '—'}</strong>
        </div>
        <div className="metric-row">
          <span>{t('admin.connectedDevices')}</span>
          <strong>{state?.connectedDevices ?? '—'}</strong>
        </div>
      </div>

      <div className="card">
        <div className="card-title">{t('admin.stats')}</div>
        <div className="metric-row">
          <span>{t('admin.rolesCount')}</span><strong>{counts.roles}</strong>
        </div>
        <div className="metric-row">
          <span>{t('admin.deviceTypesCount')}</span><strong>{counts.deviceTypes}</strong>
        </div>
        <div className="metric-row">
          <span>{t('admin.exercisesCount')}</span><strong>{counts.exercises}</strong>
        </div>
      </div>

      <div className="card" style={{ gridColumn: 'span 3' }}>
        <div className="card-title">{t('admin.recentEvents')}</div>
        {Array.isArray(state?.recentEvents) && state.recentEvents.length > 0 ? (
          <ul style={{ paddingLeft: 18, margin: 0 }}>
            {state.recentEvents.slice(0, 10).map((e, i) => (
              <li key={i} style={{ padding: '4px 0', fontSize: 14 }}>
                {typeof e === 'string' ? e : (e.message || JSON.stringify(e))}
              </li>
            ))}
          </ul>
        ) : (
          <div style={{ color: 'var(--cl-text-muted)' }}>{t('app.noData')}</div>
        )}
      </div>
    </div>
  );
}
