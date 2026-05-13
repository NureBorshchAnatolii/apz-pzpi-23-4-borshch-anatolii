import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { iotDevicesApi } from '../../api/endpoints';
import { useToast } from '../../context-helpers/useToast';
import { localeCompare } from '../../utils/format';

export default function Devices() {
  const { t, i18n } = useTranslation();
  const toast = useToast();
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    iotDevicesApi.list()
      .then((d) => setItems(Array.isArray(d) ? d : []))
      .catch(() => toast.error(t('errors.loadFailed')))
      .finally(() => setLoading(false));
  }, []);

  const sorted = [...items].sort((a, b) =>
    localeCompare(a?.name || a?.serialNumber, b?.name || b?.serialNumber, i18n.language)
  );

  if (loading) return <div className="spinner" />;
  if (sorted.length === 0) {
    return (
      <div className="card empty-state">
        <div className="empty-state-icon">📡</div>
        <div>{t('devices.empty')}</div>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-3">
      {sorted.map((d) => {
        const active = d?.isActive ?? d?.active ?? d?.state === 'active';
        return (
          <div key={d?.id ?? d?.serialNumber} className="card">
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <div>
                <div style={{ fontSize: 16, fontWeight: 700 }}>{d?.name || d?.deviceType?.name || d?.serialNumber}</div>
                <div style={{ color: 'var(--cl-text-muted)', fontSize: 13, marginTop: 4 }}>
                  {t('devices.serialNumber')}: <code>{d?.serialNumber}</code>
                </div>
                <div style={{ color: 'var(--cl-text-muted)', fontSize: 13 }}>
                  {t('devices.type')}: {d?.deviceType?.name || d?.type?.name || '—'}
                </div>
              </div>
              <span className={`badge ${active ? 'badge-success' : 'badge-muted'}`}>
                {active ? t('devices.active') : t('devices.inactive')}
              </span>
            </div>
          </div>
        );
      })}
    </div>
  );
}
