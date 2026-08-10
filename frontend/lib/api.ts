import {
  AuthResponse,
  CreateTaskRequest,
  DashboardResponse,
  LoginRequest,
  RegisterRequest,
  TaskFilters,
  TaskResponse,
  User,
} from '@/types';

const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

function getToken(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem('token');
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getToken();
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  };
  if (token) headers['Authorization'] = `Bearer ${token}`;

  const res = await fetch(`${BASE_URL}${path}`, { ...options, headers });

  if (!res.ok) {
    const err = await res.json().catch(() => ({ message: 'Error desconocido' }));
    throw new Error(err.message ?? `HTTP ${res.status}`);
  }

  // 204 No Content
  if (res.status === 204) return undefined as T;
  return res.json();
}

// ─── Auth ─────────────────────────────────────────────────────
export const authApi = {
  register: (data: RegisterRequest) =>
    request<AuthResponse>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  login: (data: LoginRequest) =>
    request<AuthResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
};

// ─── Tasks ────────────────────────────────────────────────────
export const tasksApi = {
  getAll: (filters: TaskFilters = {}) => {
    const params = new URLSearchParams();
    if (filters.status) params.set('status', filters.status);
    if (filters.priority) params.set('priority', filters.priority);
    if (filters.assignedToId) params.set('assignedToId', String(filters.assignedToId));
    const qs = params.toString();
    return request<TaskResponse[]>(`/api/tasks${qs ? `?${qs}` : ''}`);
  },

  getById: (id: number) => request<TaskResponse>(`/api/tasks/${id}`),

  create: (data: CreateTaskRequest) =>
    request<TaskResponse>('/api/tasks', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (id: number, data: CreateTaskRequest) =>
    request<TaskResponse>(`/api/tasks/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  delete: (id: number) => request<void>(`/api/tasks/${id}`, { method: 'DELETE' }),
};

// ─── Dashboard ────────────────────────────────────────────────
export const dashboardApi = {
  get: () => request<DashboardResponse>('/api/dashboard'),
};

// ─── Users ────────────────────────────────────────────────────
export const usersApi = {
  getAll: () => request<User[]>('/api/users'),
};
