import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from "react";
import { api, setAuthToken, type AuthResponse } from "../api/client";

interface CurrentUser {
  id: string;
  email: string;
  displayName: string;
}

interface AuthContextValue {
  user: CurrentUser | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, displayName: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

// Token is kept in memory only (per the spec) — a page refresh returns to login.
export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(null);

  const apply = useCallback((auth: AuthResponse) => {
    setAuthToken(auth.token);
    setUser({ id: auth.userId, email: auth.email, displayName: auth.displayName });
  }, []);

  const login = useCallback(
    async (email: string, password: string) => apply(await api.auth.login({ email, password })),
    [apply],
  );

  const register = useCallback(
    async (email: string, displayName: string, password: string) =>
      apply(await api.auth.register({ email, displayName, password })),
    [apply],
  );

  const logout = useCallback(() => {
    setAuthToken(null);
    setUser(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({ user, isAuthenticated: user !== null, login, register, logout }),
    [user, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider.");
  return ctx;
}
