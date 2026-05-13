import { useTranslation } from 'react-i18next';
import { changeLanguage } from '../i18n';

export function LangSwitch() {
  const { i18n } = useTranslation();
  const cur = i18n.language?.startsWith('ua') ? 'ua' : 'en';
  return (
    <div className="lang-switch" role="group" aria-label="Language">
      <button
        type="button"
        className={`lang-btn ${cur === 'ua' ? 'active' : ''}`}
        onClick={() => changeLanguage('ua')}
      >
        UA
      </button>
      <button
        type="button"
        className={`lang-btn ${cur === 'en' ? 'active' : ''}`}
        onClick={() => changeLanguage('en')}
      >
        EN
      </button>
    </div>
  );
}
