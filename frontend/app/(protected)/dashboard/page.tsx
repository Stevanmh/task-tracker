'use client';
import { useEffect, useState } from 'react';
import Link from 'next/link';
import { dashboardApi, tasksApi } from '@/lib/api';
import { DashboardResponse, TaskResponse } from '@/types';
import styles from './dashboard.module.css';

function MetricCard({ label, value, color }: { label: string; value: number; color: string }) {
  return (
    <div className={`card ${styles.metric}`} style={{ borderTopColor: color }}>
      <p className={styles.metricValue} style={{ color }}>{value}</p>
      <p className={styles.metricLabel}>{label}</p>
    </div>
  );
}

function statusBadge(status: string) {
  const map: Record<string, string> = {
    Pending: 'badge badge-pending',
    InProgress: 'badge badge-inprogress',
    Done: 'badge badge-done',
  };
  const labels: Record<string, string> = {
    Pending: 'Pendiente',
    InProgress: 'En progreso',
    Done: 'Hecha',
  };
  return <span className={map[status] ?? 'badge'}>{labels[status] ?? status}</span>;
}

export default function DashboardPage() {
  const [metrics, setMetrics] = useState<DashboardResponse | null>(null);
  const [recent, setRecent] = useState<TaskResponse[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([dashboardApi.get(), tasksApi.getAll()])
      .then(([m, tasks]) => {
        setMetrics(m);
        setRecent(tasks.slice(0, 5));
      })
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div className={styles.loading}>Cargando...</div>;

  return (
    <div>
      <div className={styles.header}>
        <div>
          <h1 className={styles.title}>Dashboard</h1>
          <p className={styles.subtitle}>Resumen del estado de las tareas del equipo</p>
        </div>
        <Link href="/tasks" className="btn btn-primary">
          + Nueva tarea
        </Link>
      </div>

      <div className={styles.metrics}>
        <MetricCard label="Total" value={metrics?.totalTasks ?? 0} color="var(--text-muted)" />
        <MetricCard label="Pendientes" value={metrics?.pendingTasks ?? 0} color="var(--warning)" />
        <MetricCard label="En progreso" value={metrics?.inProgressTasks ?? 0} color="var(--info)" />
        <MetricCard label="Completadas" value={metrics?.doneTasks ?? 0} color="var(--success)" />
      </div>

      <div className={styles.section}>
        <div className={styles.sectionHeader}>
          <h2 className={styles.sectionTitle}>Tareas recientes</h2>
          <Link href="/tasks" className={styles.seeAll}>Ver todas →</Link>
        </div>

        {recent.length === 0 ? (
          <div className={`card ${styles.empty}`}>
            <p>No hay tareas aún.</p>
            <Link href="/tasks" className="btn btn-primary" style={{ marginTop: '1rem' }}>
              Crear primera tarea
            </Link>
          </div>
        ) : (
          <div className={styles.taskList}>
            {recent.map(task => (
              <div key={task.id} className={`card ${styles.taskRow}`}>
                <div className={styles.taskInfo}>
                  <span className={styles.taskTitle}>{task.title}</span>
                  <span className={styles.taskMeta}>
                    {task.assignedTo ? `Asignado a: ${task.assignedTo.name}` : 'Sin asignar'}
                  </span>
                </div>
                {statusBadge(task.status)}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
