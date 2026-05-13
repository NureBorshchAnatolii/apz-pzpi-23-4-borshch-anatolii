import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { difficultiesApi, exercisesApi } from '../../api/endpoints';
import { useToast } from '../../context-helpers/useToast';
import { Modal, ConfirmDialog } from '../../components/Modal';

const empty = { title: '', description: '', difficultyId: '', typeId: '', content: '' };

export default function AdminExercises() {
  const { t } = useTranslation();
  const toast = useToast();
  const [items, setItems] = useState([]);
  const [difficulties, setDifficulties] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(null);
  const [confirmDel, setConfirmDel] = useState(null);

  function refresh() {
    setLoading(true);
    Promise.allSettled([exercisesApi.list(), difficultiesApi.list()])
      .then(([e, d]) => {
        if (e.status === 'fulfilled') setItems(Array.isArray(e.value) ? e.value : []);
        if (d.status === 'fulfilled') setDifficulties(Array.isArray(d.value) ? d.value : []);
      }).finally(() => setLoading(false));
  }
  useEffect(() => { Promise.resolve().then(refresh); }, []);

  async function save(e) {
    e.preventDefault();
    const body = {
      title: editing.title,
      description: editing.description,
      difficultyId: Number(editing.difficultyId) || 0,
      typeId: Number(editing.typeId) || 0,
      content: editing.content,
    };
    try {
      if (editing.id != null) await exercisesApi.update(editing.id, body);
      else await exercisesApi.create(body);
      toast.success(t('app.save'));
      setEditing(null);
      refresh();
    } catch {
      toast.error(t('errors.serverError'));
    }
  }

  async function doDelete() {
    try {
      await exercisesApi.remove(confirmDel);
      toast.success(t('app.delete'));
      refresh();
    } catch {
      toast.error(t('errors.serverError'));
    }
  }

  if (loading) return <div className="spinner" />;

  return (
    <div>
      <div className="page-header">
        <h2 className="page-title">{t('exercises.title')}</h2>
        <button className="btn" onClick={() => setEditing({ ...empty })}>+ {t('app.add')}</button>
      </div>

      <div className="table-wrap">
        <table className="table">
          <thead>
            <tr>
              <th>{t('exercises.exerciseTitle')}</th>
              <th>{t('exercises.difficulty')}</th>
              <th>{t('exercises.description')}</th>
              <th>{t('app.actions')}</th>
            </tr>
          </thead>
          <tbody>
            {items.length === 0 && <tr><td colSpan={4} style={{ textAlign: 'center', padding: 30 }}>{t('app.noData')}</td></tr>}
            {items.map((it) => (
              <tr key={it.id ?? it.exerciseId}>
                <td>{it.title}</td>
                <td>{it.difficulty?.name || difficulties.find((d) => (d.id ?? d.typeId) === it.difficultyId)?.name || '—'}</td>
                <td>{it.description}</td>
                <td>
                  <div className="table-actions">
                    <button className="btn btn-outline btn-sm"
                      onClick={() => setEditing({
                        id: it.id ?? it.exerciseId,
                        title: it.title || '', description: it.description || '',
                        difficultyId: it.difficultyId || '', typeId: it.typeId || '',
                        content: it.content || '',
                      })}>{t('app.edit')}</button>
                    <button className="btn btn-danger btn-sm" onClick={() => setConfirmDel(it.id ?? it.exerciseId)}>{t('app.delete')}</button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <Modal open={!!editing} onClose={() => setEditing(null)} title={editing?.id != null ? t('crud.edit') : t('crud.create')} size="lg">
        {editing && (
          <form onSubmit={save}>
            <div className="modal-body">
              <div className="form-group">
                <label className="form-label">{t('exercises.exerciseTitle')}</label>
                <input className="form-input" value={editing.title}
                  onChange={(e) => setEditing({ ...editing, title: e.target.value })} required />
              </div>
              <div className="form-group">
                <label className="form-label">{t('exercises.description')}</label>
                <textarea className="form-textarea" value={editing.description}
                  onChange={(e) => setEditing({ ...editing, description: e.target.value })} />
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label className="form-label">{t('exercises.difficulty')}</label>
                  <select className="form-select" value={editing.difficultyId}
                    onChange={(e) => setEditing({ ...editing, difficultyId: e.target.value })}>
                    <option value="">—</option>
                    {difficulties.map((d) => (
                      <option key={d.id ?? d.typeId} value={d.id ?? d.typeId}>{d.name}</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label className="form-label">{t('exercises.type')} ID</label>
                  <input type="number" className="form-input" value={editing.typeId}
                    onChange={(e) => setEditing({ ...editing, typeId: e.target.value })} />
                </div>
              </div>
              <div className="form-group">
                <label className="form-label">{t('exercises.content')}</label>
                <textarea className="form-textarea" rows={5} value={editing.content}
                  onChange={(e) => setEditing({ ...editing, content: e.target.value })} />
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
