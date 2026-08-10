'use client';
import { useEffect, useState, useCallback } from 'react';
import { tasksApi, usersApi } from '@/lib/api';
import {
  TaskResponse,
  TaskFilters,
  User,
  CreateTaskRequest,
  TaskStatus,
  TaskPriority,
} from '@/types';
import TaskModal from '@/components/TaskModal';
import styles from './tasks.module.css';

function StatusBadge({ status }: { status: TaskStatus }) {
  const map = { Pending: 'badge-pending', InProgress: 'badge-inprogress', Done: 'badge-done' };
  const labels = { Pending: 'Pendiente', InProgress: 'En progreso', Done: 'Hecha' };
  return <span className={`badge ${map[status]}`}>{labels[status]}</span>;
}

function PriorityBadge({ priority }: { priority: TaskPriority }) {
  const map = { Low: 'badge-low', Medium: 'badge-medium', High: 'badge-high' };
  const labels = { Low: 'Baja', Medium: 'Media', High: 'Alta' };
  return <span className={`badge ${map[priority]}`}>{labels[priority]}</span>;
}

export default function TasksPage() {
  const [tasks, setTasks] = useState<TaskResponse[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [filters, setFilters] = useState<TaskFilters>({});
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editTask, setEditTask] = useState<TaskResponse | null>(null);

  const loadTasks = useCallback(async () => {
    setLoading(true);
    try {
      const data = await tasksApi.getAll(filters);
      setTasks(data);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    usersApi.getAll().then(setUsers);
  }, []);
  useEffect(() => {
    loadTasks();
  }, [loadTasks]);

  async function handleDelete(id: number) {
    if (!confirm('¿Eliminar esta tarea?')) return;
    await tasksApi.delete(id);
    loadTasks();
  }

  async function handleSave(data: CreateTaskRequest) {
    if (editTask) {
      await tasksApi.update(editTask.id, data);
    } else {
      await tasksApi.create(data);
    }
    setShowModal(false);
    setEditTask(null);
    loadTasks();
  }

  function openCreate() {
    setEditTask(null);
    setShowModal(true);
  }
  function openEdit(task: TaskResponse) {
    setEditTask(task);
    setShowModal(true);
  }

  return (
    <div>
      <div className={styles.header}>
        <div>
          <h1 className={styles.title}>Tareas</h1>
          <p className={styles.subtitle}>
            {tasks.length} tarea{tasks.length !== 1 ? 's' : ''} encontrada
            {tasks.length !== 1 ? 's' : ''}
          </p>
        </div>
        <button className="btn btn-primary" onClick={openCreate}>
          + Nueva tarea
        </button>
      </div>

      {/* Filtros */}
      <div className={`card ${styles.filters}`}>
        <div className="form-group">
          <label className="form-label">Estado</label>
          <select
            value={filters.status ?? ''}
            onChange={e =>
              setFilters(f => ({ ...f, status: (e.target.value as TaskStatus) || undefined }))
            }
          >
            <option value="">Todos</option>
            <option value="Pending">Pendiente</option>
            <option value="InProgress">En progreso</option>
            <option value="Done">Hecha</option>
          </select>
        </div>
        <div className="form-group">
          <label className="form-label">Prioridad</label>
          <select
            value={filters.priority ?? ''}
            onChange={e =>
              setFilters(f => ({ ...f, priority: (e.target.value as TaskPriority) || undefined }))
            }
          >
            <option value="">Todas</option>
            <option value="Low">Baja</option>
            <option value="Medium">Media</option>
            <option value="High">Alta</option>
          </select>
        </div>
        <div className="form-group">
          <label className="form-label">Responsable</label>
          <select
            value={filters.assignedToId ?? ''}
            onChange={e =>
              setFilters(f => ({
                ...f,
                assignedToId: e.target.value ? Number(e.target.value) : undefined,
              }))
            }
          >
            <option value="">Todos</option>
            {users.map(u => (
              <option key={u.id} value={u.id}>
                {u.name}
              </option>
            ))}
          </select>
        </div>
        <button
          className="btn btn-ghost btn-sm"
          onClick={() => setFilters({})}
          style={{ alignSelf: 'flex-end' }}
        >
          Limpiar filtros
        </button>
      </div>

      {/* Lista */}
      {loading ? (
        <div className={styles.loading}>Cargando tareas...</div>
      ) : tasks.length === 0 ? (
        <div className={`card ${styles.empty}`}>
          <p>No se encontraron tareas con los filtros aplicados.</p>
        </div>
      ) : (
        <div className={styles.grid}>
          {tasks.map(task => (
            <div key={task.id} className={`card ${styles.taskCard}`}>
              <div className={styles.cardTop}>
                <div className={styles.badges}>
                  <StatusBadge status={task.status} />
                  <PriorityBadge priority={task.priority} />
                </div>
                <div className={styles.actions}>
                  <button className="btn btn-ghost btn-sm" onClick={() => openEdit(task)}>
                    Editar
                  </button>
                  <button className="btn btn-danger btn-sm" onClick={() => handleDelete(task.id)}>
                    Eliminar
                  </button>
                </div>
              </div>

              <h3 className={styles.taskTitle}>{task.title}</h3>
              {task.description && <p className={styles.taskDesc}>{task.description}</p>}

              <div className={styles.cardMeta}>
                <span>👤 {task.assignedTo?.name ?? 'Sin asignar'}</span>
                {task.deadline && (
                  <span>📅 {new Date(task.deadline).toLocaleDateString('es-CO')}</span>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {showModal && (
        <TaskModal
          task={editTask}
          users={users}
          onSave={handleSave}
          onClose={() => {
            setShowModal(false);
            setEditTask(null);
          }}
        />
      )}
    </div>
  );
}
