import { AuthResponse, User } from '@/types';

export function saveAuth(data: AuthResponse): void {
  localStorage.setItem('token', data.token);
  localStorage.setItem('user', JSON.stringify({ id: data.id, name: data.name, email: data.email }));
}

export function getUser(): User | null {
  if (typeof window === 'undefined') return null;
  const raw = localStorage.getItem('user');
  if (!raw) return null;
  try {
    return JSON.parse(raw) as User;
  } catch {
    return null;
  }
}

export function isAuthenticated(): boolean {
  if (typeof window === 'undefined') return false;
  return !!localStorage.getItem('token');
}

export function logout(): void {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
}
