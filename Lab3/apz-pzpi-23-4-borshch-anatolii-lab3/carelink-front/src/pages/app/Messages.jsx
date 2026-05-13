import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { messagesApi, relativesApi } from '../../api/endpoints';
import { useAuth } from '../../context-helpers/useAuth';
import { useToast } from '../../context-helpers/useToast';
import { ConfirmDialog } from '../../components/Modal';
import { formatDateTime } from '../../utils/format';

export default function Messages() {
  const { t, i18n } = useTranslation();
  const { user } = useAuth();
  const toast = useToast();
  const [contacts, setContacts] = useState([]);
  const [active, setActive] = useState(null);
  const [thread, setThread] = useState([]);
  const [text, setText] = useState('');
  const [loading, setLoading] = useState(false);
  const [editing, setEditing] = useState(null);
  const [confirmDelId, setConfirmDelId] = useState(null);

  useEffect(() => {
    relativesApi.list()
      .then((d) => {
        const arr = Array.isArray(d) ? d : [];
        setContacts(arr);
      })
      .catch(() => {});
  }, []);

  useEffect(() => {
    if (!active?.userId) return;
    let cancelled = false;
    messagesApi.withUser(active.userId)
      .then((d) => { if (!cancelled) setThread(Array.isArray(d) ? d : []); })
      .catch(() => { if (!cancelled) toast.error(t('errors.loadFailed')); })
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, [active?.userId]);

  function selectContact(next) {
    setThread([]);
    setLoading(true);
    setActive(next);
  }

  async function send(e) {
    e?.preventDefault();
    if (!text.trim() || !active?.userId) return;
    try {
      const created = await messagesApi.send({ content: text.trim(), receiverId: active.userId });
      setThread((arr) => [...arr, created || { id: Date.now(), content: text.trim(), senderId: user?.userId, receiverId: active.userId, createdAt: new Date().toISOString() }]);
      setText('');
    } catch {
      toast.error(t('errors.serverError'));
    }
  }

  async function saveEdit() {
    if (!editing) return;
    try {
      await messagesApi.edit(editing.id, { newContent: editing.value });
      setThread((arr) => arr.map((m) => ((m.id ?? m.messageId) === editing.id ? { ...m, content: editing.value, edited: true } : m)));
      setEditing(null);
    } catch {
      toast.error(t('errors.serverError'));
    }
  }

  async function doDelete() {
    try {
      await messagesApi.remove(confirmDelId);
      setThread((arr) => arr.filter((m) => (m.id ?? m.messageId) !== confirmDelId));
    } catch {
      toast.error(t('errors.serverError'));
    }
  }

  const sortedThread = useMemo(() => {
    return [...thread].sort((a, b) => new Date(a.sentAt || a.createdAt || a.dateTime || 0) - new Date(b.sentAt || b.createdAt || b.dateTime || 0));
  }, [thread]);

  return (
    <div className="messages-layout">
      <div className="contacts-list">
        {contacts.length === 0 && (
          <div className="empty-state" style={{ padding: 30 }}>
            <div className="empty-state-icon">👥</div>
            <div>{t('messages.noContacts')}</div>
          </div>
        )}
        {contacts.map((c) => {
          const id = c.relativeId ?? c.userId ?? c.id;
          const name = c.relativeFullName || c.fullName || `${c.firstName || ''} ${c.lastName || ''}`.trim() || `#${id}`;
          const relation = (typeof c.relationType === 'string' ? c.relationType : c.relationType?.name) || '';
          return (
            <div key={c.id ?? id} className={`contact-item ${active?.userId === id ? 'active' : ''}`}
              onClick={() => selectContact({ userId: id, name })}>
              <div className="contact-name">{name}</div>
              <div className="contact-preview">{relation}</div>
            </div>
          );
        })}
      </div>

      <div className="chat-window">
        {!active ? (
          <div className="empty-state" style={{ margin: 'auto' }}>
            <div className="empty-state-icon">💬</div>
            <div>{t('messages.noContact')}</div>
          </div>
        ) : (
          <>
            <div className="chat-header">{active.name}</div>
            <div className="chat-messages">
              {loading ? <div className="spinner" /> : sortedThread.map((m) => {
                const id = m.id ?? m.messageId;
                const mine = (m.senderId ?? m.fromUserId) === user?.userId;
                const isEditing = editing?.id === id;
                return (
                  <div key={id} className={`message-bubble ${mine ? 'mine' : 'theirs'}`}>
                    {isEditing ? (
                      <div>
                        <input className="form-input" value={editing.value}
                          onChange={(e) => setEditing({ ...editing, value: e.target.value })}
                          style={{ background: 'rgba(255,255,255,0.9)', color: '#0f172a' }} />
                        <div style={{ display: 'flex', gap: 6, marginTop: 6 }}>
                          <button type="button" className="btn btn-sm" onClick={saveEdit}>{t('app.save')}</button>
                          <button type="button" className="btn btn-ghost btn-sm" onClick={() => setEditing(null)}>{t('app.cancel')}</button>
                        </div>
                      </div>
                    ) : (
                      <>
                        <div>{m.content || m.text}</div>
                        <div className="message-time">
                          {formatDateTime(m.sentAt || m.createdAt || m.dateTime, i18n.language)}
                          {m.edited && <span> · {t('messages.edited')}</span>}
                        </div>
                        {mine && (
                          <div style={{ display: 'flex', gap: 4, marginTop: 4 }}>
                            <button type="button" className="btn btn-ghost btn-sm" style={{ color: '#fff' }}
                              onClick={() => setEditing({ id, value: m.content || m.text || '' })}>✎</button>
                            <button type="button" className="btn btn-ghost btn-sm" style={{ color: '#fff' }}
                              onClick={() => setConfirmDelId(id)}>🗑</button>
                          </div>
                        )}
                      </>
                    )}
                  </div>
                );
              })}
            </div>
            <form className="chat-input" onSubmit={send}>
              <input className="form-input" placeholder={t('messages.typeMessage')} value={text}
                onChange={(e) => setText(e.target.value)} />
              <button type="submit" className="btn">{t('app.send')}</button>
            </form>
          </>
        )}
      </div>

      <ConfirmDialog open={confirmDelId != null} onClose={() => setConfirmDelId(null)}
        onConfirm={doDelete} message={t('crud.deleteConfirm')} confirmLabel={t('app.delete')} />
    </div>
  );
}
