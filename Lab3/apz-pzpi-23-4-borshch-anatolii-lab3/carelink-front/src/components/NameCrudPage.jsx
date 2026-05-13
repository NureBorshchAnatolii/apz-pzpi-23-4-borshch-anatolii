import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Modal, ConfirmDialog } from './Modal';
import { useToast } from '../context-helpers/useToast';
import { localeCompare } from '../utils/format';

export function NameCrudPage({ api, titleKey }) {
  const { t, i18n } = useTranslation();
  const toast = useToast();
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(null);
  const [confirmId, setConfirmId] = useState(null);

  async function load() {
    setLoading(true);
    try {
      const data = await api.list();
      setItems(Array.isArray(data) ? data : []);
    } catch {
      toast.error(t('errors.loadFailed'));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { Promise.resolve().then(load); }, []);

  async function save(e) {
    e.preventDefault();
    if (!editing?.name?.trim()) return;
    try {
      if (editing.id != null) {
        await api.update(editing.id, editing.name);
      } else {
        await api.create(editing.name);
      }
      setEditing(null);
      toast.success(t('app.save'));
      load();
    } catch {
      toast.error(t('errors.serverError'));
    }
  }

  async function doRemove() {
    try {
      await api.remove(confirmId);
      toast.success(t('app.delete'));
      load();
    } catch {
      toast.error(t('errors.serverError'));
    }
  }

  const sorted = [...items].sort((a, b) => localeCompare(a?.name || a?.title, b?.name || b?.title, i18n.language));

  return (
    <div>
      <div className="page-header">
        <h2 className="page-title">{t(titleKey)}</h2>
        <button className="btn" onClick={() => setEditing({ name: '' })}>+ {t('app.add')}</button>
      </div>

      {loading ? (
        <div className="spinner" />
      ) : sorted.length === 0 ? (
        <div className="card empty-state">
          <div className="empty-state-icon">📭</div>
          <div>{t('app.noData')}</div>
        </div>
      ) : (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th style={{ width: 80 }}>ID</th>
                <th>{t('crud.name')}</th>
                <th style={{ width: 180 }}>{t('app.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {sorted.map((it) => (
                <tr key={it.id ?? it.typeId ?? it.name}>
                  <td>{it.id ?? it.typeId ?? '—'}</td>
                  <td>{it.name ?? it.title ?? ''}</td>
                  <td>
                    <div className="table-actions">
                      <button className="btn btn-outline btn-sm" onClick={() => setEditing({ id: it.id ?? it.typeId, name: it.name ?? it.title ?? '' })}>{t('app.edit')}</button>
                      <button className="btn btn-danger btn-sm" onClick={() => setConfirmId(it.id ?? it.typeId)}>{t('app.delete')}</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <Modal open={!!editing} onClose={() => setEditing(null)} title={editing?.id != null ? t('crud.edit') : t('crud.create')}>
        <form onSubmit={save}>
          <div className="modal-body">
            <div className="form-group">
              <label className="form-label">{t('crud.name')}</label>
              <input
                className="form-input"
                value={editing?.name || ''}
                onChange={(e) => setEditing({ ...editing, name: e.target.value })}
                autoFocus
                required
              />
            </div>
          </div>
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={() => setEditing(null)}>{t('app.cancel')}</button>
            <button type="submit" className="btn">{t('app.save')}</button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog
        open={confirmId != null}
        onClose={() => setConfirmId(null)}
        onConfirm={doRemove}
        message={t('crud.deleteConfirm')}
        confirmLabel={t('app.delete')}
      />
    </div>
  );
}
