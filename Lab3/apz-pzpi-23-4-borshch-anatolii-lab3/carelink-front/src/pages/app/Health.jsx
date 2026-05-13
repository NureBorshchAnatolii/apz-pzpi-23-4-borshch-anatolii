import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { iotReadingsApi } from '../../api/endpoints';
import { useAuth } from '../../context-helpers/useAuth';
import { useToast } from '../../context-helpers/useToast';
import { displayTemperature, formatDateTime, pulseStatus, temperatureStatus } from '../../utils/format';

export default function Health() {
  const { t, i18n } = useTranslation();
  const { user } = useAuth();
  const toast = useToast();
  const userId = user?.userId;
  const [latest, setLatest] = useState(null);
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');
  const [range, setRange] = useState([]);
  const [loadingLatest, setLoadingLatest] = useState(!!userId);
  const [loadingRange, setLoadingRange] = useState(false);

  useEffect(() => {
    if (!userId) return;
    iotReadingsApi.latest(userId, 1)
      .then((d) => {
        const arr = Array.isArray(d) ? d : [d];
        setLatest(arr[0] || null);
      })
      .catch(() => toast.error(t('errors.loadFailed')))
      .finally(() => setLoadingLatest(false));
  }, [userId]);

  async function loadRange() {
    if (!userId || !from || !to) return;
    setLoadingRange(true);
    try {
      const data = await iotReadingsApi.range(userId, new Date(from).toISOString(), new Date(to).toISOString());
      setRange(Array.isArray(data) ? data : []);
    } catch {
      toast.error(t('errors.loadFailed'));
    } finally {
      setLoadingRange(false);
    }
  }

  function statusBadge(level) {
    if (level === 'crit') return <span className="badge badge-danger">{t('health.critical')}</span>;
    if (level === 'warn') return <span className="badge badge-warning">{t('health.warning')}</span>;
    if (level === 'ok') return <span className="badge badge-success">{t('health.normal')}</span>;
    return <span className="badge badge-muted">—</span>;
  }

  return (
    <div>
      <div className="grid grid-cols-3" style={{ marginBottom: 18 }}>
        <div className="card">
          <div className="card-title">{t('dashboard.pulse')}</div>
          {loadingLatest ? <div className="skeleton" style={{ height: 36 }} /> : (
            <>
              <div className="metric-value">{latest?.pulse ?? '—'}</div>
              {statusBadge(pulseStatus(latest?.pulse))}
            </>
          )}
        </div>
        <div className="card">
          <div className="card-title">{t('dashboard.temperature')}</div>
          {loadingLatest ? <div className="skeleton" style={{ height: 36 }} /> : (
            <>
              <div className="metric-value">{displayTemperature(latest?.temperature)}</div>
              {statusBadge(temperatureStatus(latest?.temperature))}
            </>
          )}
        </div>
        <div className="card">
          <div className="card-title">{t('dashboard.activity')}</div>
          {loadingLatest ? <div className="skeleton" style={{ height: 36 }} /> : (
            <>
              <div className="metric-value">{latest?.activityLevel ?? '—'}</div>
              <div className="metric-label">level</div>
            </>
          )}
        </div>
      </div>

      <div className="filter-bar">
        <div className="form-group">
          <label className="form-label">{t('health.from')}</label>
          <input type="date" className="form-input" value={from} onChange={(e) => setFrom(e.target.value)} />
        </div>
        <div className="form-group">
          <label className="form-label">{t('health.to')}</label>
          <input type="date" className="form-input" value={to} onChange={(e) => setTo(e.target.value)} />
        </div>
        <button className="btn" onClick={loadRange} disabled={!from || !to || loadingRange}>
          {loadingRange ? t('app.loading') : t('app.filter')}
        </button>
      </div>

      {loadingRange ? <div className="spinner" /> : range.length === 0 ? (
        <div className="card empty-state">
          <div className="empty-state-icon">📈</div>
          <div>{t('health.noRange')}</div>
        </div>
      ) : (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>{t('health.datetime')}</th>
                <th>{t('dashboard.pulse')}</th>
                <th>{t('dashboard.temperature')}</th>
                <th>{t('dashboard.activity')}</th>
                <th>{t('health.status')}</th>
              </tr>
            </thead>
            <tbody>
              {range.map((r, i) => {
                const ps = pulseStatus(r?.pulse);
                const ts = temperatureStatus(r?.temperature);
                const worst = [ps, ts].includes('crit') ? 'crit' : [ps, ts].includes('warn') ? 'warn' : 'ok';
                return (
                  <tr key={r?.id ?? i}>
                    <td>{formatDateTime(r?.readDateTime || r?.dateTime || r?.timestamp, i18n.language)}</td>
                    <td>{r?.pulse ?? '—'}</td>
                    <td>{displayTemperature(r?.temperature)}</td>
                    <td>{r?.activityLevel ?? '—'}</td>
                    <td>{statusBadge(worst)}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
