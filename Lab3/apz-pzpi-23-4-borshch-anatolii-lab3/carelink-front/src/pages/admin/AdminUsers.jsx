import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { adminApi, rolesApi } from '../../api/endpoints';
import { useToast } from '../../context-helpers/useToast';
import { ConfirmDialog } from '../../components/Modal';
import { formatDate, localeCompare } from '../../utils/format';

export default function AdminUsers() {
  const { t, i18n } = useTranslation();
  const toast = useToast();
  const [users, setUsers] = useState([]);
  const [roles, setRoles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState('all');
  const [confirmDel, setConfirmDel] = useState(null);

  function refresh() {
    setLoading(true);
    Promise.allSettled([adminApi.listUsers(), rolesApi.list()])
      .then(([u, r]) => {
        if (u.status === 'fulfilled') setUsers(Array.isArray(u.value) ? u.value : []);
        if (r.status === 'fulfilled') setRoles(Array.isArray(r.value) ? r.value : []);
      })
      .catch(() => toast.error(t('errors.loadFailed')))
      .finally(() => setLoading(false));
  }

  useEffect(() => { Promise.resolve().then(refresh); /* eslint-disable-next-line */ }, []);

  async function setRole(userId, roleId) {
    try {
      await adminApi.setRole(userId, roleId);
      toast.success(t('app.save'));
      setUsers((arr) => arr.map((u) => ((u.id ?? u.userId) === userId ? { ...u, roleId, role: roles.find((r) => (r.id ?? r.typeId) == roleId) } : u)));
    } catch {
      toast.error(t('errors.serverError'));
    }
  }

  async function doDelete() {
    try {
      await adminApi.deleteUser(confirmDel);
      toast.success(t('app.delete'));
      setUsers((arr) => arr.filter((u) => (u.id ?? u.userId) !== confirmDel));
    } catch {
      toast.error(t('errors.serverError'));
    }
  }

  const visible = useMemo(() => {
    const q = search.trim().toLowerCase();
    return users
      .filter((u) => {
        const name = `${u.firstName || ''} ${u.lastName || ''}`.toLowerCase();
        const matchSearch = !q || name.includes(q) || (u.email || '').toLowerCase().includes(q);
        const matchRole = roleFilter === 'all' || String(u.roleId ?? u.role?.id) === String(roleFilter);
        return matchSearch && matchRole;
      })
      .sort((a, b) => localeCompare(`${a.firstName} ${a.lastName}`, `${b.firstName} ${b.lastName}`, i18n.language));
  }, [users, search, roleFilter, i18n.language]);

  if (loading) return <div className="spinner" />;

  return (
    <div>
      <div className="filter-bar">
        <div className="form-group" style={{ flex: 1 }}>
          <label className="form-label">{t('app.search')}</label>
          <input className="form-input" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="email, name..." />
        </div>
        <div className="form-group">
          <label className="form-label">{t('auth.role')}</label>
          <select className="form-select" value={roleFilter} onChange={(e) => setRoleFilter(e.target.value)}>
            <option value="all">{t('app.all')}</option>
            {roles.map((r) => (
              <option key={r.id ?? r.typeId} value={r.id ?? r.typeId}>{r.name}</option>
            ))}
          </select>
        </div>
      </div>

      <div className="table-wrap">
        <table className="table">
          <thead>
            <tr>
              <th>{t('relatives.name')}</th>
              <th>Email</th>
              <th>{t('auth.role')}</th>
              <th>{t('admin.registeredAt')}</th>
              <th>{t('app.actions')}</th>
            </tr>
          </thead>
          <tbody>
            {visible.map((u) => {
              const id = u.id ?? u.userId;
              const currentRoleId = u.roleId ?? u.role?.id ?? '';
              return (
                <tr key={id}>
                  <td>{`${u.firstName || ''} ${u.lastName || ''}`.trim() || `#${id}`}</td>
                  <td>{u.email}</td>
                  <td>
                    <select className="form-select" style={{ minWidth: 140 }}
                      value={currentRoleId}
                      onChange={(e) => setRole(id, Number(e.target.value))}>
                      <option value="">—</option>
                      {roles.map((r) => (
                        <option key={r.id ?? r.typeId} value={r.id ?? r.typeId}>{r.name}</option>
                      ))}
                    </select>
                  </td>
                  <td>{formatDate(u.createdAt || u.registeredAt, i18n.language)}</td>
                  <td>
                    <button className="btn btn-danger btn-sm" onClick={() => setConfirmDel(id)}>{t('app.delete')}</button>
                  </td>
                </tr>
              );
            })}
            {visible.length === 0 && (
              <tr><td colSpan={5} style={{ textAlign: 'center', padding: 30, color: 'var(--cl-text-muted)' }}>{t('app.noData')}</td></tr>
            )}
          </tbody>
        </table>
      </div>

      <ConfirmDialog open={confirmDel != null} onClose={() => setConfirmDel(null)}
        onConfirm={doDelete} message={t('admin.deleteUserConfirm')} confirmLabel={t('app.delete')} />
    </div>
  );
}
