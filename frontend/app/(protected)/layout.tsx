'use client';
import { useEffect, useState } from 'react';
import { isAuthenticated } from '@/lib/auth';
import Navbar from '@/components/Navbar';
import styles from './protected.module.css';

export default function ProtectedLayout({ children }: { children: React.ReactNode }) {
  const [ready, setReady] = useState(false);

  useEffect(() => {
    if (!isAuthenticated()) {
      window.location.href = '/login';
    } else {
      setReady(true);
    }
  }, []);

  if (!ready) return null;

  return (
    <div className={styles.layout}>
      <Navbar />
      <main className={styles.main}>
        <div className="container">{children}</div>
      </main>
    </div>
  );
}
