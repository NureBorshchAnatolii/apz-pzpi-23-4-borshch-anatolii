import { createContext, useEffect, useState, useCallback } from 'react';
import { jwtDecode } from 'jwt-decode';

// eslint-disable-next-line react-refresh/only-export-components
export const AuthContext = createContext(null);

const TOKEN_KEY = 'carelink_token';
const USER_KEY = 'carelink_user';

function decodeUser(token) {
  try {
    const payload = jwtDecode(token);
    const userId =
      payload?.userId ||
      payload?.sub ||
      payload?.nameid ||
      payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
    const role =
      payload?.role ||
      payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      payload?.roles;
    const email =
      payload?.email ||
      payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];
    return {
      userId: userId ? Number(userId) || userId : null,
      role: Array.isArray(role) ? role[0] : role,
      email,
      raw: payload,
    };
  } catch {
    return null;
  }
}

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem(TOKEN_KEY));
  const [user, setUser] = useState(() => {
    const raw = localStorage.getItem(USER_KEY);
    if (raw) {
      try { return JSON.parse(raw); } catch { /* ignore */ }
    }
    const t = localStorage.getItem(TOKEN_KEY);
    return t ? decodeUser(t) : null;
  });

  const login = useCallback((newToken, extra = {}) => {
    localStorage.setItem(TOKEN_KEY, newToken);
    const decoded = decodeUser(newToken) || {};
    const merged = { ...decoded, ...extra };
    localStorage.setItem(USER_KEY, JSON.stringify(merged));
    setToken(newToken);
    setUser(merged);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    setToken(null);
    setUser(null);
  }, []);

  const updateUser = useCallback((patch) => {
    setUser((prev) => {
      const next = { ...(prev || {}), ...patch };
      localStorage.setItem(USER_KEY, JSON.stringify(next));
      return next;
    });
  }, []);

  useEffect(() => {
    function onStorage(e) {
      if (e.key === TOKEN_KEY) {
        setToken(e.newValue);
        if (!e.newValue) setUser(null);
      }
    }
    window.addEventListener('storage', onStorage);
    return () => window.removeEventListener('storage', onStorage);
  }, []);

  const value = {
    token,
    user,
    isAuthenticated: !!token,
    isAdmin: typeof user?.role === 'string' && ['admin', 'administrator'].includes(user.role.toLowerCase()),
    login,
    logout,
    updateUser,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
