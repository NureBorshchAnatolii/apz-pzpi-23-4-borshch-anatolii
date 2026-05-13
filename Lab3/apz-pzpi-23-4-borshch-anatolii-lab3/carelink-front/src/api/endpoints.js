import { apiClient } from './client';

const unwrap = (res) => {
  const data = res?.data;
  if (data && typeof data === 'object' && 'data' in data && !Array.isArray(data)) {
    return data.data;
  }
  return data;
};

export const authApi = {
  login: (body) => apiClient.post('/api/v1/auth/login', body).then(unwrap),
  register: (body) => apiClient.post('/api/v1/auth/register', body).then(unwrap),
  logout: () => apiClient.post('/api/v1/auth/logout').then(unwrap),
};

export const usersApi = {
  getById: (id) => apiClient.get(`/api/v1/users/${id}`).then(unwrap),
  me: () => apiClient.get('/api/v1/users').then(unwrap),
  update: (body) => apiClient.put('/api/v1/users', body).then(unwrap),
  remove: () => apiClient.delete('/api/v1/users').then(unwrap),
};

export const adminApi = {
  systemState: () => apiClient.get('/api/admin/system-state').then(unwrap),
  listUsers: () => apiClient.get('/api/admin/users').then(unwrap),
  setRole: (userId, roleId) => apiClient.put(`/api/admin/users/${userId}/role/${roleId}`).then(unwrap),
  deleteUser: (userId) => apiClient.delete(`/api/admin/users/${userId}`).then(unwrap),
  logs: (params) => apiClient.get('/api/admin/logs', { params }).then(unwrap),
};

const crud = (base) => ({
  list: () => apiClient.get(base).then(unwrap),
  create: (body) => apiClient.post(base, body).then(unwrap),
  update: (id, body) => apiClient.put(`${base}/${id}`, body).then(unwrap),
  remove: (id) => apiClient.delete(`${base}/${id}`).then(unwrap),
});

export const rolesApi = crud('/api/roles');
export const notificationTypesApi = crud('/api/notiffication-types'); // typo in API
export const deviceTypesApi = crud('/api/device-types');
export const difficultiesApi = crud('/api/difficulties');
export const relationTypesApi = crud('/api/relation-types');

export const notificationsApi = {
  list: () => apiClient.get('/api/notifications').then(unwrap),
  markRead: (id) => apiClient.put(`/api/notifications/${id}`).then(unwrap),
};

export const messagesApi = {
  withUser: (receiverId) => apiClient.get(`/api/messages/user/${receiverId}`).then(unwrap),
  send: (body) => apiClient.post('/api/messages', body).then(unwrap),
  edit: (id, body) => apiClient.put(`/api/messages/${id}`, body).then(unwrap),
  remove: (id) => apiClient.delete(`/api/messages/${id}`).then(unwrap),
};

export const iotDevicesApi = {
  list: () => apiClient.get('/api/iot-devices').then(unwrap),
  create: (body) => apiClient.post('/api/iot-devices', body).then(unwrap),
  update: (deviceId, body) => apiClient.put(`/api/iot-devices/${deviceId}`, body).then(unwrap),
  getState: (serialNumber) => apiClient.get(`/api/iot-devices/${serialNumber}/state`).then(unwrap),
  setState: (serialNumber) => apiClient.put(`/api/iot-devices/${serialNumber}/state`).then(unwrap),
};

export const iotReadingsApi = {
  forUser: (userId) => apiClient.get(`/api/iot-readings/${userId}`).then(unwrap),
  latest: (userId, count = 1) => apiClient.get(`/api/iot-readings/${userId}/latest`, { params: { count } }).then(unwrap),
  range: (userId, from, to) => apiClient.get(`/api/iot-readings/${userId}/range`, { params: { from, to } }).then(unwrap),
};

export const exercisesApi = {
  list: () => apiClient.get('/api/cognitive-exercise').then(unwrap),
  create: (body) => apiClient.post('/api/cognitive-exercise', body).then(unwrap),
  update: (id, body) => apiClient.put(`/api/cognitive-exercise/${id}`, body).then(unwrap),
  remove: (id) => apiClient.delete(`/api/cognitive-exercise/${id}`).then(unwrap),
  saveResult: (exerciseId, body) => apiClient.post(`/api/cognitive-exercise/${exerciseId}/result`, body).then(unwrap),
  results: (userId) => apiClient.get(`/api/cognitive-exercise/${userId}/result`).then(unwrap),
};

export const relativesApi = {
  list: () => apiClient.get('/api/relatives').then(unwrap),
  create: (body) => apiClient.post('/api/relatives', body).then(unwrap),
  remove: (id) => apiClient.delete(`/api/relatives/${id}`).then(unwrap),
  report: (id) => apiClient.get(`/api/relatives/${id}/report`).then(unwrap),
};
