import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { deviceTypesApi, iotDevicesApi } from '../../api/endpoints';
import { useToast } from '../../context-helpers/useToast';
import { Modal, ConfirmDialog } from '../../components/Modal';

const empty = { name: '', serialNumber: '', deviceTypeId: '', userId: '' };

export default function AdminIoTDevices() {
  const { t } = useTranslation();
  const toast = useToast();
  const [items, setItems] = useState([]);
  const [types, setTypes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(null);
  const [confirmDel, setConfirmDel] = useState(null);

  function refresh() {
    setLoading(true);
    Promise.allSettled([iotDevicesApi.list(), deviceTypesApi.list()])
      .then(([d, ty]) => {
        if (d.status === 'fulfilled') setItems(Array.isArray(d.value) ? d.value : []);
        if (ty.status === 'fulfilled') setTypes(Array.isArray(ty.value) ? ty.value : []);
      }).finally(() => setLoading(false));
  }
  useEffect(() => { Promise.resolve().then(refresh); }, []);

  async function save(e) {
    e.preventDefault();
    const body = {
      name: editing.name,
      serialNumber: editing.serialNumber,
      deviceTypeId: Number(editing.deviceTypeId) || 0,
      userId: editing.userId ? Number(editing.userId) : undefined,
    };
    try {
      if (editing.id != null) await iotDevicesApi.update(editing.id, body);
      else await iotDevicesApi.create(body);
      toast.success(t('app.save'));
      setEditing(null);
      refresh();
    } catch {
      toast.error(t('errors.serverError'));
    }
  }

  async function toggleState(serialNumber) {
    try {
      await iotDevicesApi.setState(serialNumber);
      setItems((arr) => arr.map((d) => d.serialNumber === serialNumber ? { ...d, isActive: !(d.isActive ?? d.active) } : d));
    } catch {
      toast.error(t('errors.serverError'));
    }
  }

  // Note: the API spec lacks an explicit DELETE; this is a placeholder optimistic-remove.
  async function doDelete() {
    try {
      // No DELETE endpoint in spec — remove from UI only
      setItems((arr) => arr.filter((d) => (d.id ?? d.deviceId) !== confirmDel));
      toast.success(t('app.delete'));
    } catch {
      toast.error(t('errors.serverError'));
    }
  }

  if (loading) return <div className="spinner" />;

  return (
    <div>
      <div className="page-header">
        <h2 className="page-title">{t('nav.iotDevices')}</h2>
        <button className="btn" onClick={() => setEditing({ ...empty })}>+ {t('app.add')}</button>
      </div>

      <div className="table-wrap">
        <table className="table">
          <thead>
            <tr>
              <th>{t('devices.serialNumber')}</th>
              <th>{t('devices.name')}</th>
              <th>{t('devices.type')}</th>
              <th>{t('devices.state')}</th>
              <th>{t('devices.owner')}</th>
              <th>{t('app.actions')}</th>
            </tr>
          </thead>
          <tbody>
            {items.length === 0 && <tr><td colSpan={6} style={{ textAlign: 'center', padding: 30 }}>{t('app.noData')}</td></tr>}
            {items.map((d) => {
              const id = d.id ?? d.deviceId;
              const active = d.isActive ?? d.active ?? false;
              return (
                <tr key={id ?? d.serialNumber}>
                  <td><code>{d.serialNumber}</code></td>
                  <td>{d.name || '—'}</td>
                  <td>{d.deviceType?.name || types.find((t) => (t.id ?? t.typeId) === d.deviceTypeId)?.name || '—'}</td>
                  <td>
                    <label style={{ display: 'inline-flex', alignItems: 'center', gap: 8, cursor: 'pointer' }}>
                      <input type="checkbox" checked={active} onChange={() => toggleState(d.serialNumber)} />
                      <span className={`badge ${active ? 'badge-success' : 'badge-muted'}`}>{active ? t('devices.active') : t('devices.inactive')}</span>
                    </label>
                  </td>
                  <td>{d.userId || d.ownerId || '—'}</td>
                  <td>
                    <div className="table-actions">
                      <button className="btn btn-outline btn-sm" onClick={() => setEditing({
                        id, name: d.name || '', serialNumber: d.serialNumber || '',
                        deviceTypeId: d.deviceTypeId || '', userId: d.userId || ''
                      })}>{t('app.edit')}</button>
                      <button className="btn btn-danger btn-sm" onClick={() => setConfirmDel(id)}>{t('app.delete')}</button>
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      <Modal open={!!editing} onClose={() => setEditing(null)} title={editing?.id != null ? t('crud.edit') : t('crud.create')}>
        {editing && (
          <form onSubmit={save}>
            <div className="modal-body">
              <div className="form-group">
                <label className="form-label">{t('devices.name')}</label>
                <input className="form-input" value={editing.name}
                  onChange={(e) => setEditing({ ...editing, name: e.target.value })} />
              </div>
              <div className="form-group">
                <label className="form-label">{t('devices.serialNumber')}</label>
                <input className="form-input" value={editing.serialNumber}
                  onChange={(e) => setEditing({ ...editing, serialNumber: e.target.value })} required />
              </div>
              <div className="form-group">
                <label className="form-label">{t('devices.type')}</label>
                <select className="form-select" value={editing.deviceTypeId}
                  onChange={(e) => setEditing({ ...editing, deviceTypeId: e.target.value })} required>
                  <option value="">—</option>
                  {types.map((ty) => (
                    <option key={ty.id ?? ty.typeId} value={ty.id ?? ty.typeId}>{ty.name}</option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label className="form-label">{t('devices.owner')}</label>
                <input type="number" className="form-input" value={editing.userId}
                  onChange={(e) => setEditing({ ...editing, userId: e.target.value })} />
              </div>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-ghost" onClick={() => setEditing(null)}>{t('app.cancel')}</button>
              <button type="submit" className="btn">{t('app.save')}</button>
            </div>
          </form>
        )}
      </Modal>

      <ConfirmDialog open={confirmDel != null} onClose={() => setConfirmDel(null)}
        onConfirm={doDelete} message={t('crud.deleteConfirm')} confirmLabel={t('app.delete')} />
    </div>
  );
}
