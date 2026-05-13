import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { adminApi } from '../../api/endpoints';
import { useToast } from '../../context-helpers/useToast';
import { formatDateTime } from '../../utils/format';

export default function AdminLogs() {
  const { t, i18n } = useTranslation();
  const toast = useToast();
  const [logs, setLogs] = useState([]);
  const [filters, setFilters] = useState({ from: '', to: '', level: 'all' });
  const [loading, setLoading] = useState(true);

  function load() {
    setLoading(true);
    const params = {};
    if (filters.from) params.from = new Date(filters.from).toISOString();
    if (filters.to) params.to = new Date(filters.to).toISOString();
    if (filters.level && filters.level !== 'all') params.level = filters.level;
    adminApi.logs(params)
      .then((d) => setLogs(Array.isArray(d) ? d : []))
      .catch(() => toast.error(t('errors.loadFailed')))
      .finally(() => setLoading(false));
  }

  useEffect(() => { Promise.resolve().then(load); /* eslint-disable-next-line */ }, []);

  function badge(level) {
    const l = String(level || '').toLowerCase();
    if (l.includes('err')) return <span className="badge badge-danger">{t('admin.error')}</span>;
    if (l.includes('warn')) return <span className="badge badge-warning">{t('admin.warn')}</span>;
    return <span className="badge badge-info">{t('admin.info')}</span>;
  }

  const sorted = [...logs].sort((a, b) => new Date(b.timestamp || b.dateTime || 0) - new Date(a.timestamp || a.dateTime || 0));

  return (
    <div>
      <div className="filter-bar">
        <div className="form-group">
          <label className="form-label">{t('admin.level')}</label>
          <select className="form-select" value={filters.level} onChange={(e) => setFilters({ ...filters, level: e.target.value })}>
            <option value="all">{t('app.all')}</option>
            <option value="info">{t('admin.info')}</option>
            <option value="warn">{t('admin.warn')}</option>
            <option value="error">{t('admin.error')}</option>
          </select>
        </div>
        <div className="form-group">
          <label className="form-label">{t('health.from')}</label>
          <input type="date" className="form-input" value={filters.from} onChange={(e) => setFilters({ ...filters, from: e.target.value })} />
        </div>
        <div className="form-group">
          <label className="form-label">{t('health.to')}</label>
          <input type="date" className="form-input" value={filters.to} onChange={(e) => setFilters({ ...filters, to: e.target.value })} />
        </div>
        <button className="btn" onClick={load}>{t('app.filter')}</button>
      </div>

      {loading ? <div className="spinner" /> : sorted.length === 0 ? (
        <div className="card empty-state">{t('app.noData')}</div>
      ) : (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th style={{ width: 180 }}>{t('health.datetime')}</th>
                <th style={{ width: 110 }}>{t('admin.level')}</th>
                <th>{t('admin.message')}</th>
                <th style={{ width: 160 }}>{t('admin.user')}</th>
              </tr>
            </thead>
            <tbody>
              {sorted.map((l, i) => (
                <tr key={l.id ?? i}>
                  <td>{formatDateTime(l.timestamp || l.dateTime, i18n.language)}</td>
                  <td>{badge(l.level)}</td>
                  <td>{l.message || l.text}</td>
                  <td>{l.user?.email || l.userId || '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
