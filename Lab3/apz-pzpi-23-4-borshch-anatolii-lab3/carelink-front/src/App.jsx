import { BrowserRouter, Routes, Route, Navigate, useNavigate } from 'react-router-dom';
import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { AuthProvider } from './auth/AuthContext';
import { useAuth } from './context-helpers/useAuth';
import { ToastProvider } from './components/Toast' 
import { useToast } from './context-helpers/useToast';
import { PrivateRoute } from './auth/PrivateRoute';
import { Layout } from './components/Layout';
import { configureApiHandlers } from './api/client';

import Login from './pages/Login';
import Register from './pages/Register';
import { NotFound, Forbidden } from './pages/Errors';

import Dashboard from './pages/app/Dashboard';
import Notifications from './pages/app/Notifications';
import Messages from './pages/app/Messages';
import Health from './pages/app/Health';
import Devices from './pages/app/Devices';
import Exercises from './pages/app/Exercises';
import Relatives from './pages/app/Relatives';
import Profile from './pages/app/Profile';

import AdminDashboard from './pages/admin/AdminDashboard';
import AdminUsers from './pages/admin/AdminUsers';
import AdminLogs from './pages/admin/AdminLogs';
import AdminExercises from './pages/admin/AdminExercises';
import AdminIoTDevices from './pages/admin/AdminIoTDevices';
import AdminBackup from './pages/admin/AdminBackup';
import { NameCrudPage } from './components/NameCrudPage';
import {
  rolesApi, notificationTypesApi, deviceTypesApi, difficultiesApi, relationTypesApi,
} from './api/endpoints';

function ApiBootstrap() {
  const { logout } = useAuth();
  const navigate = useNavigate();
  const toast = useToast();
  const { t } = useTranslation();
  useEffect(() => {
    configureApiHandlers({
      onUnauthorized: () => { logout(); navigate('/login', { replace: true }); },
      onForbidden: () => navigate('/forbidden', { replace: true }),
      onServerError: () => toast.error(t('errors.serverError')),
    });
  }, [logout, navigate, toast, t]);
  return null;
}

export default function App() {
  return (
    <BrowserRouter>
      <ToastProvider>
        <AuthProvider>
          <ApiBootstrap />
          <Routes>
            <Route path="/" element={<Navigate to="/login" replace />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/forbidden" element={<Forbidden />} />

            <Route path="/app" element={<PrivateRoute><Layout /></PrivateRoute>}>
              <Route index element={<Navigate to="dashboard" replace />} />
              <Route path="dashboard" element={<Dashboard />} />
              <Route path="notifications" element={<Notifications />} />
              <Route path="messages" element={<Messages />} />
              <Route path="health" element={<Health />} />
              <Route path="devices" element={<Devices />} />
              <Route path="exercises" element={<Exercises />} />
              <Route path="relatives" element={<Relatives />} />
              <Route path="profile" element={<Profile />} />
            </Route>

            <Route path="/admin" element={<PrivateRoute adminOnly><Layout admin /></PrivateRoute>}>
              <Route index element={<Navigate to="dashboard" replace />} />
              <Route path="dashboard" element={<AdminDashboard />} />
              <Route path="users" element={<AdminUsers />} />
              <Route path="logs" element={<AdminLogs />} />
              <Route path="roles" element={<NameCrudPage key="roles" api={rolesApi} titleKey="nav.roles" />} />
              <Route path="notification-types" element={<NameCrudPage key="notification-types" api={notificationTypesApi} titleKey="nav.notificationTypes" />} />
              <Route path="device-types" element={<NameCrudPage key="device-types" api={deviceTypesApi} titleKey="nav.deviceTypes" />} />
              <Route path="difficulties" element={<NameCrudPage key="difficulties" api={difficultiesApi} titleKey="nav.difficulties" />} />
              <Route path="relation-types" element={<NameCrudPage key="relation-types" api={relationTypesApi} titleKey="nav.relationTypes" />} />
              <Route path="exercises" element={<AdminExercises />} />
              <Route path="iot-devices" element={<AdminIoTDevices />} />
              <Route path="backup" element={<AdminBackup />} />
            </Route>

            <Route path="*" element={<NotFound />} />
          </Routes>
        </AuthProvider>
      </ToastProvider>
    </BrowserRouter>
  );
}
