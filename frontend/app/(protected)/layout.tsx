'use client';
import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { isAuthenticated } from '@/lib/auth';
import Navbar from '@/components/Navbar';
import styles from './protected.module.css';

export default function ProtectedLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();

  useEffect(() => {
    if (!isAuthenticated()) {
      router.replace('/login');
    }
  }, [router]);

  if (typeof window !== 'undefined' && !isAuthenticated()) return null;

  return (
    <div className={styles.layout}>
      <Navbar />
      <main className={styles.main}>
        <div className="container">{children}</div>
      </main>
    </div>
  );
}
