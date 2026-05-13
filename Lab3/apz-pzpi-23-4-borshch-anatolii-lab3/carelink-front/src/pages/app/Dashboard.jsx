import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context-helpers/useAuth';
import { iotReadingsApi, notificationsApi, exercisesApi } from '../../api/endpoints';
import { displayTemperature, pulseStatus, temperatureStatus } from '../../utils/format';

function Skeleton({ height = 60 }) { return <div className="skeleton" style={{ height, width: '100%' }} />; }

export default function Dashboard() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const navigate = useNavigate();
  const userId = user?.userId;
  const [loading, setLoading] = useState(!!userId);
  const [latest, setLatest] = useState(null);
  const [unread, setUnread] = useState(0);
  const [recentNotifications, setRecentNotifications] = useState([]);
  const [weeklyExercises, setWeeklyExercises] = useState(0);

  useEffect(() => {
    if (!userId) return;
    Promise.allSettled([
      iotReadingsApi.latest(userId, 1),
      notificationsApi.list(),
      exercisesApi.results(userId),
    ]).then(([latestRes, notifRes, resultsRes]) => {
      if (latestRes.status === 'fulfilled') {
        const arr = Array.isArray(latestRes.value) ? latestRes.value : [latestRes.value];
        setLatest(arr[0] || null);
      }
      if (notifRes.status === 'fulfilled') {
        const arr = Array.isArray(notifRes.value) ? notifRes.value : [];
        setRecentNotifications(arr.slice(0, 5));
        setUnread(arr.filter((n) => !n?.isRead && !n?.read).length);
      }
      if (resultsRes.status === 'fulfilled') {
        const arr = Array.isArray(resultsRes.value) ? resultsRes.value : [];
        const weekAgo = Date.now() - 7 * 24 * 3600 * 1000;
        setWeeklyExercises(arr.filter((r) => new Date(r?.completedAt).getTime() >= weekAgo).length);
      }
    }).finally(() => setLoading(false));
  }, [userId]);

  return (
    <div className="grid grid-cols-2">
      <div className="card" onClick={() => navigate('/app/health')} style={{ cursor: 'pointer' }}>
        <div className="card-title">{t('dashboard.latestHealth')}</div>
        {loading ? <Skeleton height={80} /> : (
          <>
            <div className="metric-row">
              <span><span className={`status-dot ${pulseStatus(latest?.pulse)}`} />{t('dashboard.pulse')}</span>
              <span className="metric-value" style={{ fontSize: 20 }}>{latest?.pulse ?? '—'}</span>
            </div>
            <div className="metric-row">
              <span><span className={`status-dot ${temperatureStatus(latest?.temperature)}`} />{t('dashboard.temperature')}</span>
              <span className="metric-value" style={{ fontSize: 20 }}>{displayTemperature(latest?.temperature)}</span>
            </div>
            <div className="metric-row">
              <span>{t('dashboard.activity')}</span>
              <span className="metric-value" style={{ fontSize: 20 }}>{latest?.activityLevel ?? '—'}</span>
            </div>
          </>
        )}
      </div>

      <div className="card" onClick={() => navigate('/app/messages')} style={{ cursor: 'pointer' }}>
        <div className="card-title">{t('dashboard.unreadMessages')}</div>
        {loading ? <Skeleton height={80} /> : (
          <>
            <div className="metric-value">{unread}</div>
            <div className="metric-label">{t('nav.messages')}</div>
          </>
        )}
      </div>

      <div className="card" onClick={() => navigate('/app/notifications')} style={{ cursor: 'pointer' }}>
        <div className="card-title">{t('dashboard.activeNotifications')}</div>
        {loading ? <Skeleton height={80} /> : recentNotifications.length === 0 ? (
          <div style={{ color: 'var(--cl-text-muted)' }}>{t('dashboard.noNotifications')}</div>
        ) : (
          <ul style={{ paddingLeft: 18, margin: 0 }}>
            {recentNotifications.map((n, i) => (
              <li key={n?.id ?? i} style={{ padding: '4px 0', fontSize: 14 }}>
                {n?.title || n?.text || n?.message || `#${n?.id}`}
              </li>
            ))}
          </ul>
        )}
      </div>

      <div className="card" onClick={() => navigate('/app/exercises')} style={{ cursor: 'pointer' }}>
        <div className="card-title">{t('dashboard.weeklyExercises')}</div>
        {loading ? <Skeleton height={80} /> : (
          <>
            <div className="metric-value">{weeklyExercises}</div>
            <div className="metric-label">{t('nav.exercises')}</div>
          </>
        )}
      </div>
    </div>
  );
}
