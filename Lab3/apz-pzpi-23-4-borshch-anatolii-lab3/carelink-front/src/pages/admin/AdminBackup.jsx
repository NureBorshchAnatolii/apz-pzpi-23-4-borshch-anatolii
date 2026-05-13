import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useToast } from '../../context-helpers/useToast';
import { ConfirmDialog } from '../../components/Modal';
import { formatDateTime } from '../../utils/format';
import {
  adminApi, iotDevicesApi, exercisesApi, notificationsApi,
} from '../../api/endpoints';

const STORAGE_KEY = 'carelink_backups_mock';

function loadBackups() {
  try { return JSON.parse(localStorage.getItem(STORAGE_KEY) || '[]'); } catch { return []; }
}
function saveBackups(arr) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(arr));
}

const datasets = {
  users: { label: 'backup.users', loader: () => adminApi.listUsers() },
  devices: { label: 'backup.devices', loader: () => iotDevicesApi.list() },
  exercises: { label: 'backup.exercisesData', loader: () => exercisesApi.list() },
  notifications: { label: 'backup.notifications', loader: () => notificationsApi.list() },
};

function toCSV(rows) {
  if (!Array.isArray(rows) || rows.length === 0) return '';
  const keys = Array.from(new Set(rows.flatMap((r) => Object.keys(r || {}))));
  const escape = (v) => {
    const s = v == null ? '' : typeof v === 'object' ? JSON.stringify(v) : String(v);
    return `"${s.replace(/"/g, '""')}"`;
  };
  return [keys.join(','), ...rows.map((r) => keys.map((k) => escape(r[k])).join(','))].join('\n');
}

function downloadFile(filename, content, mime) {
  const blob = new Blob([content], { type: mime });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url; a.download = filename;
  document.body.appendChild(a); a.click();
  document.body.removeChild(a);
  setTimeout(() => URL.revokeObjectURL(url), 0);
}

export default function AdminBackup() {
  const { t, i18n } = useTranslation();
  const toast = useToast();
  const [backups, setBackups] = useState(loadBackups());
  const [exportType, setExportType] = useState('users');
  const [format, setFormat] = useState('json');
  const [busy, setBusy] = useState(false);
  const [confirmImport, setConfirmImport] = useState(null); // { name, parsed }

  useEffect(() => { saveBackups(backups); }, [backups]);

  function createBackup() {
    const name = `backup_${new Date().toISOString().replace(/[:.]/g, '-')}.json`;
    setBusy(true);
    Promise.allSettled(Object.values(datasets).map((d) => d.loader()))
      .then((results) => {
        const data = {
          users: results[0].status === 'fulfilled' ? results[0].value : [],
          devices: results[1].status === 'fulfilled' ? results[1].value : [],
          exercises: results[2].status === 'fulfilled' ? results[2].value : [],
          notifications: results[3].status === 'fulfilled' ? results[3].value : [],
          createdAt: new Date().toISOString(),
        };
        const content = JSON.stringify(data, null, 2);
        const size = new Blob([content]).size;
        setBackups((arr) => [{ name, content, size, createdAt: data.createdAt }, ...arr]);
        toast.success(t('app.save'));
      })
      .finally(() => setBusy(false));
  }

  function downloadBackup(b) {
    downloadFile(b.name, b.content, 'application/json');
  }

  async function doExport() {
    setBusy(true);
    try {
      let payload;
      if (exportType === 'all') {
        const all = await Promise.allSettled(Object.values(datasets).map((d) => d.loader()));
        payload = Object.fromEntries(Object.keys(datasets).map((k, i) => [k, all[i].status === 'fulfilled' ? all[i].value : []]));
      } else {
        payload = await datasets[exportType].loader();
      }
      const empty = Array.isArray(payload) ? payload.length === 0 : Object.values(payload).every((v) => Array.isArray(v) && v.length === 0);
      if (empty) { toast.info(t('backup.exportEmpty')); return; }
      const fname = `${exportType}_${new Date().toISOString().slice(0, 10)}.${format}`;
      if (format === 'json') downloadFile(fname, JSON.stringify(payload, null, 2), 'application/json');
      else {
        const rows = Array.isArray(payload) ? payload : Object.entries(payload).flatMap(([k, v]) => (Array.isArray(v) ? v.map((r) => ({ _dataset: k, ...r })) : []));
        downloadFile(fname, toCSV(rows), 'text/csv');
      }
      toast.success(t('backup.export'));
    } catch {
      toast.error(t('errors.serverError'));
    } finally {
      setBusy(false);
    }
  }

  function onFileSelected(e) {
    const file = e.target.files?.[0];
    e.target.value = '';
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      try {
        const text = String(reader.result || '');
        const parsed = file.name.endsWith('.json') ? JSON.parse(text) : { csv: text };
        setConfirmImport({ name: file.name, parsed });
      } catch {
        toast.error(t('errors.serverError'));
      }
    };
    reader.readAsText(file);
  }

  function doImport() {
    toast.success(t('backup.imported'));
    setConfirmImport(null);
  }

  return (
    <div className="grid">
      <div className="card">
        <div className="card-title">{t('backup.backups')}</div>
        <div style={{ marginBottom: 12 }}>
          <button className="btn" onClick={createBackup} disabled={busy}>+ {t('backup.createBackup')}</button>
        </div>
        {backups.length === 0 ? (
          <div className="empty-state">{t('app.noData')}</div>
        ) : (
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr><th>{t('backup.backupName')}</th><th>{t('backup.createdAt')}</th><th>{t('backup.size')}</th><th>{t('app.actions')}</th></tr>
              </thead>
              <tbody>
                {backups.map((b, i) => (
                  <tr key={i}>
                    <td><code>{b.name}</code></td>
                    <td>{formatDateTime(b.createdAt, i18n.language)}</td>
                    <td>{(b.size / 1024).toFixed(1)} KB</td>
                    <td>
                      <div className="table-actions">
                        <button className="btn btn-outline btn-sm" onClick={() => downloadBackup(b)}>{t('backup.download')}</button>
                        <button className="btn btn-danger btn-sm" onClick={() => setBackups((arr) => arr.filter((_, j) => j !== i))}>{t('app.delete')}</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <div className="grid grid-cols-2">
        <div className="card">
          <div className="card-title">{t('backup.exportData')}</div>
          <div className="form-group">
            <label className="form-label">{t('backup.exportWhat')}</label>
            <select className="form-select" value={exportType} onChange={(e) => setExportType(e.target.value)}>
              {Object.keys(datasets).map((k) => <option key={k} value={k}>{t(datasets[k].label)}</option>)}
              <option value="all">{t('backup.all')}</option>
            </select>
          </div>
          <div className="form-group">
            <label className="form-label">{t('backup.exportFormat')}</label>
            <select className="form-select" value={format} onChange={(e) => setFormat(e.target.value)}>
              <option value="json">JSON</option>
              <option value="csv">CSV</option>
            </select>
          </div>
          <button className="btn" onClick={doExport} disabled={busy}>{t('backup.export')}</button>
        </div>

        <div className="card">
          <div className="card-title">{t('backup.import')}</div>
          <div className="form-group">
            <label className="form-label">{t('backup.selectFile')}</label>
            <input type="file" accept=".json,.csv" className="form-input" onChange={onFileSelected} />
          </div>
          {confirmImport && (
            <div style={{ background: 'var(--cl-bg)', padding: 10, borderRadius: 8, fontSize: 12, maxHeight: 180, overflow: 'auto' }}>
              <strong>{confirmImport.name}</strong>
              <pre style={{ margin: '6px 0 0', whiteSpace: 'pre-wrap' }}>
                {JSON.stringify(confirmImport.parsed, null, 2).slice(0, 500)}{JSON.stringify(confirmImport.parsed).length > 500 ? '...' : ''}
              </pre>
            </div>
          )}
        </div>
      </div>

      <ConfirmDialog open={!!confirmImport} onClose={() => setConfirmImport(null)}
        onConfirm={doImport} message={t('backup.importConfirm')} confirmLabel={t('backup.importBtn')} />
    </div>
  );
}
