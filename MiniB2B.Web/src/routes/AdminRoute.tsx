import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export function AdminRoute() {
  const { user } = useAuth();

  if (user?.role !== 'Admin') {
    return <Navigate to="/forbidden" replace />;
  }

  return <Outlet />;
}
