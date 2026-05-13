import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../context-helpers/useAuth';
import { LangSwitch } from './LangSwitch';

const userNav = [
  { to: '/app/dashboard', key: 'nav.dashboard' },
  { to: '/app/notifications', key: 'nav.notifications' },
  { to: '/app/messages', key: 'nav.messages' },
  { to: '/app/health', key: 'nav.health' },
  { to: '/app/devices', key: 'nav.devices' },
  { to: '/app/exercises', key: 'nav.exercises' },
  { to: '/app/relatives', key: 'nav.relatives' },
  { to: '/app/profile', key: 'nav.profile' },
];

const adminNav = [
  { to: '/admin/dashboard', key: 'nav.adminDashboard' },
  { to: '/admin/users', key: 'nav.users' },
  { to: '/admin/logs', key: 'nav.logs' },
  { to: '/admin/roles', key: 'nav.roles' },
  { to: '/admin/notification-types', key: 'nav.notificationTypes' },
  { to: '/admin/device-types', key: 'nav.deviceTypes' },
  { to: '/admin/difficulties', key: 'nav.difficulties' },
  { to: '/admin/relation-types', key: 'nav.relationTypes' },
  { to: '/admin/exercises', key: 'nav.exercises' },
  { to: '/admin/iot-devices', key: 'nav.iotDevices' },
  { to: '/admin/backup', key: 'nav.backup' },
];

export function Layout({ admin = false }) {
  const { t } = useTranslation();
  const { logout, isAdmin } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const items = admin ? adminNav : userNav;
  const current = items.find((i) => location.pathname.startsWith(i.to));
  const title = current ? t(current.key) : '';

  function handleLogout() {
    logout();
    navigate('/login', { replace: true });
  }

  return (
    <div className="app-layout">
      <aside className="sidebar">
        <div className="sidebar-logo">
          <div className="auth-logo-icon" style={{ width: 36, height: 36, fontSize: 16 }}>C</div>
          <div className="sidebar-logo-text">{t('app.name')}</div>
        </div>
        <nav>
          {items.map((it) => (
            <NavLink key={it.to} to={it.to} className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
              <span className="nav-icon">{it.icon}</span>
              <span>{t(it.key)}</span>
            </NavLink>
          ))}
          {!admin && isAdmin && (
            <NavLink to="/admin/dashboard" className="nav-link" style={{ marginTop: 14, color: 'var(--cl-accent)' }}>
              <span className="nav-icon">⚙️</span>
              <span>Admin</span>
            </NavLink>
          )}
          {admin && (
            <NavLink to="/app/dashboard" className="nav-link" style={{ marginTop: 14 }}>
              <span className="nav-icon">↩️</span>
              <span>{t('nav.dashboard')}</span>
            </NavLink>
          )}
        </nav>
      </aside>
      <div className="main-area">
        <header className="app-header">
          <h1 className="app-header-title">{title}</h1>
          <div className="app-header-right">
            <LangSwitch />
            <button className="btn btn-outline btn-sm" onClick={handleLogout}>{t('app.logout')}</button>
          </div>
        </header>
        <main className="app-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
