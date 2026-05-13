import axios from 'axios';

export const API_BASE_URL = 'http://localhost:5000';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('carelink_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

let onUnauthorized = null;
let onForbidden = null;
let onServerError = null;

export function configureApiHandlers(handlers) {
  onUnauthorized = handlers.onUnauthorized;
  onForbidden = handlers.onForbidden;
  onServerError = handlers.onServerError;
}

apiClient.interceptors.response.use(
  (res) => res,
  (err) => {
    const status = err?.response?.status;
    if (status === 401 && onUnauthorized) onUnauthorized();
    else if (status === 403 && onForbidden) onForbidden();
    else if (status >= 500 && onServerError) onServerError();
    return Promise.reject(err);
  }
);
