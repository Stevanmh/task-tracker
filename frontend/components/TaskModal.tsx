'use client';
import { useState, useEffect } from 'react';
import { TaskResponse, User, CreateTaskRequest, TaskStatus, TaskPriority } from '@/types';
import styles from './TaskModal.module.css';

interface Props {
  task: TaskResponse | null;
  users: User[];
  onSave: (data: CreateTaskRequest) => Promise<void>;
  onClose: () => void;
}

export default function TaskModal({ task, users, onSave, onClose }: Props) {
  const [title, setTitle] = useState(task?.title ?? '');
  const [description, setDescription] = useState(task?.description ?? '');
  const [status, setStatus] = useState<TaskStatus>(task?.status ?? 'Pending');
  const [priority, setPriority] = useState<TaskPriority>(task?.priority ?? 'Medium');
  const [assignedToId, setAssignedToId] = useState<number | undefined>(task?.assignedTo?.id);
  const [deadline, setDeadline] = useState(
    task?.deadline ? task.deadline.split('T')[0] : ''
  );
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    function onKey(e: KeyboardEvent) { if (e.key === 'Escape') onClose(); }
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, [onClose]);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!title.trim()) { setError('El título es requerido'); return; }
    setError('');
    setSaving(true);
    try {
      await onSave({
        title: title.trim(),
        description: description.trim() || undefined,
        status,
        priority,
        assignedToId,
        deadline: deadline || undefined,
      });
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error al guardar');
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="modal-overlay" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="modal">
        <div className={styles.header}>
          <h2 className={styles.title}>{task ? 'Editar tarea' : 'Nueva tarea'}</h2>
          <button className={styles.closeBtn} onClick={onClose} aria-label="Cerrar">✕</button>
        </div>

        <form onSubmit={handleSubmit} className={styles.form}>
          <div className="form-group">
            <label className="form-label" htmlFor="task-title">Título *</label>
            <input
              id="task-title"
              type="text"
              placeholder="¿En qué hay que trabajar?"
              value={title}
              onChange={e => setTitle(e.target.value)}
              required
              maxLength={200}
              autoFocus
            />
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="task-desc">Descripción</label>
            <textarea
              id="task-desc"
              placeholder="Detalles opcionales..."
              value={description}
              onChange={e => setDescription(e.target.value)}
              rows={3}
              maxLength={2000}
              style={{ resize: 'vertical' }}
            />
          </div>

          <div className={styles.row}>
            <div className="form-group">
              <label className="form-label" htmlFor="task-status">Estado</label>
              <select id="task-status" value={status} onChange={e => setStatus(e.target.value as TaskStatus)}>
                <option value="Pending">Pendiente</option>
                <option value="InProgress">En progreso</option>
                <option value="Done">Hecha</option>
              </select>
            </div>
            <div className="form-group">
              <label className="form-label" htmlFor="task-priority">Prioridad</label>
              <select id="task-priority" value={priority} onChange={e => setPriority(e.target.value as TaskPriority)}>
                <option value="Low">Baja</option>
                <option value="Medium">Media</option>
                <option value="High">Alta</option>
              </select>
            </div>
          </div>

          <div className={styles.row}>
            <div className="form-group">
              <label className="form-label" htmlFor="task-assignee">Responsable</label>
              <select
                id="task-assignee"
                value={assignedToId ?? ''}
                onChange={e => setAssignedToId(e.target.value ? Number(e.target.value) : undefined)}
              >
                <option value="">Sin asignar</option>
                {users.map(u => <option key={u.id} value={u.id}>{u.name}</option>)}
              </select>
            </div>
            <div className="form-group">
              <label className="form-label" htmlFor="task-deadline">Fecha límite</label>
              <input
                id="task-deadline"
                type="date"
                value={deadline}
                onChange={e => setDeadline(e.target.value)}
                min={new Date().toISOString().split('T')[0]}
              />
            </div>
          </div>

          {error && <p className="form-error">{error}</p>}

          <div className={styles.footer}>
            <button type="button" className="btn btn-ghost" onClick={onClose}>
              Cancelar
            </button>
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving ? 'Guardando...' : task ? 'Guardar cambios' : 'Crear tarea'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
