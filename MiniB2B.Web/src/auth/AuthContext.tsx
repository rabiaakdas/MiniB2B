// oxlint-disable react/set-state-in-effect
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { apiClient } from '../api/apiClient';
import { subscribeToAuthExpired } from './authEvents';
import { tokenStorage } from './tokenStorage';
import type { AuthResponse, CurrentUser, LoginRequest, MeResponse, RegisterRequest } from '../types/auth';

// oxlint-disable react/only-export-components
interface AuthContextValue {
  user: CurrentUser | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (request: LoginRequest) => Promise<CurrentUser>;
  register: (request: RegisterRequest) => Promise<CurrentUser>;
  logout: () => void;
  refreshCurrentUser: () => Promise<void>;
  updateCurrentUser: (user: CurrentUser) => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function mapAuthResponse(response: AuthResponse): CurrentUser {
  return {
    userId: response.userId,
    firstName: response.firstName,
    lastName: response.lastName,
    email: response.email,
    role: response.role,
  };
}

function mapMeResponse(response: MeResponse): CurrentUser {
  return {
    userId: Number(response.userId),
    firstName: response.firstName,
    lastName: response.lastName,
    email: response.email,
    role: response.role,
  };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [token, setToken] = useState<string | null>(() => tokenStorage.get());
  const [isLoading, setIsLoading] = useState(true);

  const logout = useCallback(() => {
    tokenStorage.clear();
    setToken(null);
    setUser(null);
  }, []);

  const updateCurrentUser = useCallback((currentUser: CurrentUser) => {
    setUser(currentUser);
  }, []);

  const refreshCurrentUser = useCallback(async () => {
    const storedToken = tokenStorage.get();

    if (!storedToken) {
      setUser(null);
      setToken(null);
      return;
    }

    try {
      const response = await apiClient.get<MeResponse>('/api/auth/me');
      setUser(mapMeResponse(response.data));
      setToken(storedToken);
    } catch {
      tokenStorage.clear();
      setUser(null);
      setToken(null);
    }
  }, []);

  useEffect(() => {
    refreshCurrentUser().finally(() => {
      setIsLoading(false);
    });
  }, [refreshCurrentUser]);

  useEffect(() => {
    return subscribeToAuthExpired(() => {
      setUser(null);
      setToken(null);
    });
  }, []);

  const login = useCallback(async (request: LoginRequest) => {
    const response = await apiClient.post<AuthResponse>('/api/auth/login', request);
    tokenStorage.set(response.data.token);
    setToken(response.data.token);
    const currentUser = mapAuthResponse(response.data);
    setUser(currentUser);
    return currentUser;
  }, []);

  const register = useCallback(async (request: RegisterRequest) => {
    const response = await apiClient.post<AuthResponse>('/api/auth/register', request);
    tokenStorage.set(response.data.token);
    setToken(response.data.token);
    const currentUser = mapAuthResponse(response.data);
    setUser(currentUser);
    return currentUser;
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      token,
      isAuthenticated: Boolean(user && token),
      isLoading,
      login,
      register,
      logout,
      refreshCurrentUser,
      updateCurrentUser,
    }),
    [user, token, isLoading, login, register, logout, refreshCurrentUser, updateCurrentUser],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within AuthProvider.');
  }

  return context;
}
