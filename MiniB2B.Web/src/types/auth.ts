export type UserRole = 'Admin' | 'User';

export interface AuthResponse {
  token: string;
  expiresAt: string;
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  role: UserRole;
}

export interface CurrentUser {
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  role: UserRole;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  password: string;
}

export interface MeResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
}

export interface UserAccount {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
}

export interface UpdateUserAccountRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
}

export interface ChangeUserPasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}
