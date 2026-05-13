import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context-helpers/useAuth';

export function PrivateRoute({ children, adminOnly = false }) {
  const { isAuthenticated, isAdmin } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }
  if (adminOnly && !isAdmin) {
    return <Navigate to="/app/dashboard" replace />;
  }
  return children;
}
