// ─── Auth ─────────────────────────────────────────────────────
export interface User {
  id: number;
  name: string;
  email: string;
}

export interface AuthResponse {
  id: number;
  name: string;
  email: string;
  token: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

// ─── Tasks ────────────────────────────────────────────────────
export type TaskStatus = 'Pending' | 'InProgress' | 'Done';
export type TaskPriority = 'Low' | 'Medium' | 'High';

export interface TaskResponse {
  id: number;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  deadline?: string;
  createdAt: string;
  updatedAt: string;
  createdBy: User;
  assignedTo?: User;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  assignedToId?: number;
  deadline?: string;
}

export interface UpdateTaskRequest extends CreateTaskRequest {}

// ─── Dashboard ────────────────────────────────────────────────
export interface DashboardResponse {
  totalTasks: number;
  pendingTasks: number;
  inProgressTasks: number;
  doneTasks: number;
}

// ─── Filters ──────────────────────────────────────────────────
export interface TaskFilters {
  status?: TaskStatus;
  priority?: TaskPriority;
  assignedToId?: number;
}
