import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  title: 'Task Tracker — Jiro',
  description: 'Gestión de tareas de equipo con seguimiento de estado, prioridad y responsables',
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="es">
      <body>{children}</body>
    </html>
  );
}
