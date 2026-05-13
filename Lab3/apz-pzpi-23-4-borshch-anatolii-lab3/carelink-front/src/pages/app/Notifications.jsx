import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { notificationsApi } from '../../api/endpoints';
import { useToast } from '../../context-helpers/useToast';
import { formatDateTime } from '../../utils/format';

export default function Notifications() {
  const { t, i18n } = useTranslation();
  const toast = useToast();
  const [items, setItems] = useState([]);
  const [filter, setFilter] = useState('all');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    notificationsApi.list()
      .then((d) => setItems(Array.isArray(d) ? d : []))
      .catch(() => toast.error(t('errors.loadFailed')))
      .finally(() => setLoading(false));
  }, []);

  async function markRead(id) {
    try {
      await notificationsApi.markRead(id);
      setItems((arr) => arr.map((n) => ((n.id ?? n.notificationId) === id ? { ...n, isRead: true, read: true } : n)));
    } catch {
      toast.error(t('errors.serverError'));
    }
  }

  const visible = useMemo(() => {
    const list = items.map((n) => ({ ...n, _id: n.id ?? n.notificationId, _read: n.isRead || n.read }));
    const filtered = list.filter((n) => filter === 'all' ? true : filter === 'unread' ? !n._read : n._read);
    return filtered.sort((a, b) => {
      if (a._read !== b._read) return a._read ? 1 : -1;
      const da = new Date(a.createdAt || a.dateTime || 0).getTime();
      const db = new Date(b.createdAt || b.dateTime || 0).getTime();
      return db - da;
    });
  }, [items, filter]);

  return (
    <div>
      <div className="filter-bar">
        <div className="form-group">
          <label className="form-label">{t('app.filter')}</label>
          <select className="form-select" value={filter} onChange={(e) => setFilter(e.target.value)}>
            <option value="all">{t('app.all')}</option>
            <option value="unread">{t('notifications.unread')}</option>
            <option value="read">{t('notifications.read')}</option>
          </select>
        </div>
      </div>

      {loading ? <div className="spinner" /> : visible.length === 0 ? (
        <div className="card empty-state">
          <div className="empty-state-icon">🔔</div>
          <div>{t('notifications.empty')}</div>
        </div>
      ) : (
        <div className="grid">
          {visible.map((n) => (
            <div key={n._id} className="card" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                  <span style={{ fontSize: 20 }}>{n.type?.icon || '🔔'}</span>
                  <strong>{n.title || n.notificationType?.name || `#${n._id}`}</strong>
                  {!n._read && <span className="badge badge-info">{t('notifications.unread')}</span>}
                </div>
                <div style={{ color: 'var(--cl-text-muted)', marginTop: 4 }}>
                  {n.text || n.message || n.content}
                </div>
                <div style={{ color: 'var(--cl-text-soft)', fontSize: 12, marginTop: 6 }}>
                  {formatDateTime(n.createdAt || n.dateTime, i18n.language)}
                </div>
              </div>
              {!n._read && (
                <button className="btn btn-outline btn-sm" onClick={() => markRead(n._id)}>{t('notifications.markRead')}</button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
