'use client';
import Link from 'next/link';
import { useRouter, usePathname } from 'next/navigation';
import { logout, getUser } from '@/lib/auth';
import styles from './Navbar.module.css';

export default function Navbar() {
  const router = useRouter();
  const pathname = usePathname();
  const user = getUser();

  function handleLogout() {
    logout();
    router.push('/login');
  }

  return (
    <nav className={styles.nav}>
      <div className={`container ${styles.inner}`}>
        <Link href="/dashboard" className={styles.logo}>
          <span className={styles.logoIcon}>✦</span>
          Task Tracker
        </Link>

        <div className={styles.links}>
          <Link
            href="/dashboard"
            className={`${styles.link} ${pathname === '/dashboard' ? styles.active : ''}`}
          >
            Dashboard
          </Link>
          <Link
            href="/tasks"
            className={`${styles.link} ${pathname === '/tasks' ? styles.active : ''}`}
          >
            Tareas
          </Link>
        </div>

        <div className={styles.user}>
          <span className={styles.userName}>{user?.name}</span>
          <button className="btn btn-ghost btn-sm" onClick={handleLogout}>
            Salir
          </button>
        </div>
      </div>
    </nav>
  );
}
