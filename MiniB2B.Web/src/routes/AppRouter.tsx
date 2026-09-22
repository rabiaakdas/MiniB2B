import { Navigate, Route, Routes } from 'react-router-dom';
import { AdminLayout } from '../layouts/AdminLayout';
import { AdminAccountPage } from '../pages/AdminAccountPage';
import { AuthLayout } from '../layouts/AuthLayout';
import { UserLayout } from '../layouts/UserLayout';
import { AdminDashboardPage } from '../pages/AdminDashboardPage';
import { AdminBannersPage } from '../pages/AdminBannersPage';
import { AdminOrdersPage } from '../pages/AdminOrdersPage';
import { AdminProductsPage } from '../pages/AdminProductsPage';
import { AdminUsersPage } from '../pages/AdminUsersPage';
import { AccountPage } from '../pages/AccountPage';
import { CartPage } from '../pages/CartPage';
import { ForbiddenPage } from '../pages/ForbiddenPage';
import { HomePage } from '../pages/HomePage';
import { LoginPage } from '../pages/LoginPage';
import { NotFoundPage } from '../pages/NotFoundPage';
import { ProductsPage } from '../pages/ProductsPage';
import { OrdersPage } from '../pages/OrdersPage';
import { RegisterPage } from '../pages/RegisterPage';
import { AdminRoute } from './AdminRoute';
import { ProtectedRoute } from './ProtectedRoute';

export function AppRouter() {
  return (
    <Routes>
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
      </Route>

      <Route element={<UserLayout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/products" element={<ProductsPage />} />
        <Route path="/forbidden" element={<ForbiddenPage />} />

        <Route element={<ProtectedRoute />}>
          <Route path="/account" element={<AccountPage />} />
          <Route path="/cart" element={<CartPage />} />
          <Route path="/orders" element={<OrdersPage />} />
        </Route>
      </Route>

      <Route element={<ProtectedRoute />}>
        <Route element={<AdminRoute />}>
          <Route path="/admin" element={<AdminLayout />}>
            <Route index element={<AdminDashboardPage />} />
            <Route path="account" element={<AdminAccountPage />} />
            <Route path="products" element={<AdminProductsPage />} />
            <Route path="orders" element={<AdminOrdersPage />} />
            <Route path="users" element={<AdminUsersPage />} />
            <Route path="banners" element={<AdminBannersPage />} />
          </Route>
        </Route>
      </Route>

      <Route path="/not-found" element={<NotFoundPage />} />
      <Route path="*" element={<Navigate to="/not-found" replace />} />
    </Routes>
  );
}
