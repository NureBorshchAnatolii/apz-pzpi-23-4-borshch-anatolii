import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { difficultiesApi, exercisesApi } from '../../api/endpoints';
import { useAuth } from '../../context-helpers/useAuth';
import { useToast } from '../../context-helpers/useToast';
import { Modal } from '../../components/Modal';
import { formatDateTime } from '../../utils/format';

export default function Exercises() {
  const { t, i18n } = useTranslation();
  const { user } = useAuth();
  const toast = useToast();
  const [items, setItems] = useState([]);
  const [difficulties, setDifficulties] = useState([]);
  const [results, setResults] = useState([]);
  const [diffFilter, setDiffFilter] = useState('all');
  const [active, setActive] = useState(null);
  const [score, setScore] = useState(0);
  const [loading, setLoading] = useState(true);

  function refresh() {
    setLoading(true);
    Promise.allSettled([
      exercisesApi.list(),
      difficultiesApi.list(),
      user?.userId ? exercisesApi.results(user.userId) : Promise.resolve([]),
    ]).then(([ex, di, re]) => {
      if (ex.status === 'fulfilled') setItems(Array.isArray(ex.value) ? ex.value : []);
      if (di.status === 'fulfilled') setDifficulties(Array.isArray(di.value) ? di.value : []);
      if (re.status === 'fulfilled') setResults(Array.isArray(re.value) ? re.value : []);
    }).finally(() => setLoading(false));
  }

  useEffect(() => { Promise.resolve().then(refresh); /* eslint-disable-next-line */ }, [user?.userId]);

  const visible = useMemo(() => {
    if (diffFilter === 'all') return items;
    return items.filter((e) => String(e?.difficultyId ?? e?.difficulty?.id) === String(diffFilter));
  }, [items, diffFilter]);

  async function saveResult() {
    if (!active) return;
    try {
      await exercisesApi.saveResult(active.id ?? active.exerciseId, {
        userId: user?.userId,
        score: Number(score) || 0,
        completedAt: new Date().toISOString(),
      });
      toast.success(t('app.save'));
      setActive(null);
      setScore(0);
      refresh();
    } catch {
      toast.error(t('errors.serverError'));
    }
  }

  if (loading) return <div className="spinner" />;

  return (
    <div>
      <div className="filter-bar">
        <div className="form-group">
          <label className="form-label">{t('exercises.difficulty')}</label>
          <select className="form-select" value={diffFilter} onChange={(e) => setDiffFilter(e.target.value)}>
            <option value="all">{t('app.all')}</option>
            {difficulties.map((d) => (
              <option key={d.id ?? d.typeId} value={d.id ?? d.typeId}>{d.name}</option>
            ))}
          </select>
        </div>
      </div>

      <div className="grid grid-cols-2" style={{ marginBottom: 20 }}>
        {visible.length === 0 ? (
          <div className="card empty-state" style={{ gridColumn: '1 / -1' }}>{t('app.noData')}</div>
        ) : visible.map((e) => (
          <div key={e.id ?? e.exerciseId} className="card">
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <div>
                <div style={{ fontSize: 16, fontWeight: 700 }}>{e.title}</div>
                <div style={{ color: 'var(--cl-text-muted)', fontSize: 13, marginTop: 4 }}>{e.description}</div>
              </div>
              <span className="badge badge-info">{e.difficulty?.name || difficulties.find(d => (d.id ?? d.typeId) === e.difficultyId)?.name || '—'}</span>
            </div>
            <div style={{ marginTop: 12 }}>
              <button className="btn" onClick={() => { setActive(e); setScore(0); }}>{t('exercises.play')}</button>
            </div>
          </div>
        ))}
      </div>

      <h3 className="card-title" style={{ color: 'var(--cl-primary-dark)' }}>{t('exercises.myResults')}</h3>
      {results.length === 0 ? (
        <div className="card empty-state">{t('exercises.noResults')}</div>
      ) : (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>{t('exercises.exerciseTitle')}</th>
                <th>{t('exercises.score')}</th>
                <th>{t('exercises.completedAt')}</th>
              </tr>
            </thead>
            <tbody>
              {results.map((r, i) => (
                <tr key={r?.id ?? i}>
                  <td>{r?.exercise?.title || items.find((x) => (x.id ?? x.exerciseId) === r?.exerciseId)?.title || `#${r?.exerciseId}`}</td>
                  <td><strong>{r?.score}</strong></td>
                  <td>{formatDateTime(r?.completedAt, i18n.language)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <Modal open={!!active} onClose={() => setActive(null)} title={active?.title}>
        <div className="modal-body">
          <p style={{ color: 'var(--cl-text-muted)' }}>{t('exercises.exerciseRunning')}</p>
          <div style={{ background: 'var(--cl-bg)', padding: 14, borderRadius: 8, marginBottom: 14 }}>
            {active?.content || active?.description}
          </div>
          <div className="form-group">
            <label className="form-label">{t('exercises.score')}</label>
            <input type="number" min="0" className="form-input" value={score}
              onChange={(e) => setScore(e.target.value)} />
          </div>
        </div>
        <div className="modal-footer">
          <button className="btn btn-ghost" onClick={() => setActive(null)}>{t('app.cancel')}</button>
          <button className="btn" onClick={saveResult}>{t('exercises.saveResult')}</button>
        </div>
      </Modal>
    </div>
  );
}
