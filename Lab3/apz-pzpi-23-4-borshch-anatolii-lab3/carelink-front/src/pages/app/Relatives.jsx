import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { relativesApi, relationTypesApi } from '../../api/endpoints';
import { useToast } from '../../context-helpers/useToast';
import { Modal, ConfirmDialog } from '../../components/Modal';

export default function Relatives() {
  const { t } = useTranslation();
  const toast = useToast();
  const [items, setItems] = useState([]);
  const [types, setTypes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [adding, setAdding] = useState(false);
  const [form, setForm] = useState({ relativeId: '', relationTypeId: '' });
  const [confirmDel, setConfirmDel] = useState(null);
  const [report, setReport] = useState(null);

  function refresh() {
    setLoading(true);
    Promise.allSettled([relativesApi.list(), relationTypesApi.list()])
      .then(([r, ty]) => {
        if (r.status === 'fulfilled') setItems(Array.isArray(r.value) ? r.value : []);
        if (ty.status === 'fulfilled') setTypes(Array.isArray(ty.value) ? ty.value : []);
      }).finally(() => setLoading(false));
  }
  useEffect(() => { Promise.resolve().then(refresh); }, []);

  async function add(e) {
    e.preventDefault();
    if (!form.relativeId || !form.relationTypeId) return;
    try {
      await relativesApi.create({
        relativeId: Number(form.relativeId),
        relationTypeId: Number(form.relationTypeId),
      });
      toast.success(t('app.save'));
      setAdding(false);
      setForm({ relativeId: '', relationTypeId: '' });
      refresh();
    } catch {
      toast.error(t('errors.serverError'));
    }
  }

  async function doRemove() {
    try {
      await relativesApi.remove(confirmDel);
      toast.success(t('app.delete'));
      refresh();
    } catch {
      toast.error(t('errors.serverError'));
    }
  }

  async function showReport(id) {
    try {
      const data = await relativesApi.report(id);
      setReport({ id, data });
    } catch {
      toast.error(t('errors.loadFailed'));
    }
  }

  if (loading) return <div className="spinner" />;

  return (
    <div>
      <div className="page-header">
        <h2 className="page-title">{t('relatives.title')}</h2>
        <button className="btn" onClick={() => setAdding(true)}>+ {t('relatives.addRelative')}</button>
      </div>

      {items.length === 0 ? (
        <div className="card empty-state">
          <div className="empty-state-icon">👨‍👩‍👧</div>
          <div>{t('relatives.empty')}</div>
        </div>
      ) : (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>{t('relatives.name')}</th>
                <th>{t('relatives.relationType')}</th>
                <th>{t('relatives.addedAt')}</th>
                <th>{t('app.actions')}</th>
              </tr>
            </thead>
            <tbody>
              {items.map((r) => {
                const rowId = r.id ?? r.relativeId;
                const name = r.relativeFullName || r.fullName || `${r.firstName || ''} ${r.lastName || ''}`.trim() || `#${r.relativeId ?? rowId}`;
                const relation = (typeof r.relationType === 'string' ? r.relationType : r.relationType?.name) || r.relationTypeName || '—';
                const addedAt = r.addedAt ? new Date(r.addedAt).toLocaleDateString() : '—';
                return (
                  <tr key={rowId}>
                    <td>{name}</td>
                    <td>{relation}</td>
                    <td>{addedAt}</td>
                    <td>
                      <div className="table-actions">
                        <button className="btn btn-outline btn-sm" onClick={() => showReport(rowId)}>{t('app.report')}</button>
                        <button className="btn btn-danger btn-sm" onClick={() => setConfirmDel(rowId)}>{t('app.delete')}</button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      <Modal open={adding} onClose={() => setAdding(false)} title={t('relatives.addRelative')}>
        <form onSubmit={add}>
          <div className="modal-body">
            <div className="form-group">
              <label className="form-label">{t('relatives.relativeUserId')}</label>
              <input type="number" className="form-input" value={form.relativeId}
                onChange={(e) => setForm({ ...form, relativeId: e.target.value })} required />
            </div>
            <div className="form-group">
              <label className="form-label">{t('relatives.relationType')}</label>
              <select className="form-select" value={form.relationTypeId}
                onChange={(e) => setForm({ ...form, relationTypeId: e.target.value })} required>
                <option value="">—</option>
                {types.map((ty) => (
                  <option key={ty.id ?? ty.typeId} value={ty.id ?? ty.typeId}>{ty.name}</option>
                ))}
              </select>
            </div>
          </div>
          <div className="modal-footer">
            <button type="button" className="btn btn-ghost" onClick={() => setAdding(false)}>{t('app.cancel')}</button>
            <button type="submit" className="btn">{t('app.save')}</button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog open={confirmDel != null} onClose={() => setConfirmDel(null)}
        onConfirm={doRemove} message={t('relatives.deleteConfirm')} confirmLabel={t('app.delete')} />

      <Modal open={!!report} onClose={() => setReport(null)} title={`${t('app.report')} #${report?.id}`} size="lg">
        <div className="modal-body">
          <pre style={{ background: 'var(--cl-bg)', padding: 14, borderRadius: 8, overflowX: 'auto', fontSize: 12 }}>
            {JSON.stringify(report?.data, null, 2)}
          </pre>
        </div>
        <div className="modal-footer">
          <button className="btn" onClick={() => setReport(null)}>{t('app.close')}</button>
        </div>
      </Modal>
    </div>
  );
}
