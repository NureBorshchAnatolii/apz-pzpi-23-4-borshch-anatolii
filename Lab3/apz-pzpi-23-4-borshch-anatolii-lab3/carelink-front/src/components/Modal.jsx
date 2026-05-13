import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';

export function Modal({ open, onClose, title, children, size = '' }) {
  useEffect(() => {
    function onKey(e) { if (e.key === 'Escape') onClose?.(); }
    if (open) document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, [open, onClose]);

  if (!open) return null;
  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className={`modal ${size === 'lg' ? 'modal-lg' : ''}`} onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3 className="modal-title">{title}</h3>
          <button className="modal-close" onClick={onClose} aria-label="Close">×</button>
        </div>
        {children}
      </div>
    </div>
  );
}

export function ConfirmDialog({ open, onClose, onConfirm, title, message, confirmLabel, danger = true }) {
  const { t } = useTranslation();
  if (!open) return null;
  return (
    <Modal open={open} onClose={onClose} title={title || t('app.confirm')}>
      <div className="modal-body">
        <p style={{ margin: 0, color: 'var(--cl-text-muted)' }}>{message}</p>
      </div>
      <div className="modal-footer">
        <button className="btn btn-ghost" onClick={onClose}>{t('app.cancel')}</button>
        <button className={`btn ${danger ? 'btn-danger' : ''}`} onClick={() => { onConfirm?.(); onClose?.(); }}>
          {confirmLabel || t('app.confirm')}
        </button>
      </div>
    </Modal>
  );
}
