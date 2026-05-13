import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import en from '../../public/locales/en/translation.json';
import ua from '../../public/locales/ua/translation.json';

const STORAGE_KEY = 'carelink_lang';
const stored = typeof localStorage !== 'undefined' ? localStorage.getItem(STORAGE_KEY) : null;
const initial = stored || 'ua';

i18n.use(initReactI18next).init({
  lng: initial,
  fallbackLng: 'en',
  supportedLngs: ['ua', 'en'],
  resources: {
    en: { translation: en },
    ua: { translation: ua },
  },
  interpolation: { escapeValue: false },
  react: { useSuspense: false },
});

export function changeLanguage(lng) {
  localStorage.setItem(STORAGE_KEY, lng);
  i18n.changeLanguage(lng);
}

export default i18n;
