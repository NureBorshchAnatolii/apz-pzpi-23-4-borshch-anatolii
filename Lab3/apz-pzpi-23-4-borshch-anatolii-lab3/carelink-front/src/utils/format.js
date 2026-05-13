export function formatDate(value, locale) {
  if (!value) return '';
  const date = value instanceof Date ? value : new Date(value);
  if (isNaN(date.getTime())) return '';
  if (locale === 'ua') {
    const d = String(date.getDate()).padStart(2, '0');
    const m = String(date.getMonth() + 1).padStart(2, '0');
    return `${d}.${m}.${date.getFullYear()}`;
  }
  const d = String(date.getDate()).padStart(2, '0');
  const m = String(date.getMonth() + 1).padStart(2, '0');
  return `${m}/${d}/${date.getFullYear()}`;
}

export function formatDateTime(value, locale) {
  if (!value) return '';
  const date = value instanceof Date ? value : new Date(value);
  if (isNaN(date.getTime())) return '';
  const datePart = formatDate(date, locale);
  const h = String(date.getHours()).padStart(2, '0');
  const m = String(date.getMinutes()).padStart(2, '0');
  return `${datePart} ${h}:${m}`;
}

export function localeCompare(a, b, locale) {
  return String(a || '').localeCompare(String(b || ''), locale === 'ua' ? 'uk' : 'en');
}

export function pulseStatus(value) {
  if (value == null) return 'muted';
  if (value < 50 || value > 110) return 'crit';
  if (value < 60 || value > 100) return 'warn';
  return 'ok';
}

export function temperatureStatus(value) {
  if (value == null) return 'muted';
  const v = value > 100 ? value / 10 : value;
  if (v < 35 || v > 38.5) return 'crit';
  if (v < 36 || v > 37.5) return 'warn';
  return 'ok';
}

export function displayTemperature(value) {
  if (value == null) return '—';
  const v = value > 100 ? value / 10 : value;
  return `${v.toFixed(1)}°C`;
}
