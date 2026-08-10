# ADR-005 — Arquitectura del frontend: Next.js App Router + CSS Modules

**Fecha:** 2026-08-10
**Estado:** Aceptado
**Originado por:** Requisito de la prueba: "frontend con Next.js que consuma la API"

## Contexto

Se debía construir el frontend completo: autenticación, dashboard con métricas, gestión de tareas con filtros y CRUD. Se necesitaba decidir cómo organizar rutas, proteger páginas privadas y manejar estilos.

---

## Decisión 1 — Rutas protegidas: App Router con grupo `(protected)`

### Alternativas consideradas

| Opción | Descripción | Problema |
|:---|:---|:---|
| Middleware global (`middleware.ts`) | Intercepta todas las requests y redirige si no hay token | Requiere leer cookies; JWT en `localStorage` no es accesible desde middleware de servidor |
| Layout de grupo `(protected)` | Un `layout.tsx` que verifica el token en el cliente antes de renderizar | Funciona con `localStorage`; toda la lógica de auth en un solo lugar |
| HOC (Higher-Order Component) | Envuelve cada página en un componente de auth | Código repetido en cada página protegida |

**Decisión:** Layout de grupo `(protected)/layout.tsx` con verificación en cliente.

**Justificación:** El token JWT se guarda en `localStorage` por simplicidad (sin necesidad de cookies HTTP-only para el alcance de esta prueba). El layout de grupo centraliza la lógica de redirección en un solo archivo: si no hay token → redirige a `/login`. Todas las páginas dentro del grupo heredan esta protección sin código adicional.

**Impacto en el código:** `frontend/app/(protected)/layout.tsx` con `useEffect` + `useRouter` del cliente.

---

## Decisión 2 — Estilos: CSS Modules vs Tailwind CSS

### Alternativas consideradas

| Opción | Pros | Contras |
|:---|:---|:---|
| **CSS Modules** | Encapsulamiento por componente, CSS estándar legible, sin dependencias extra | Más archivos `.module.css` |
| **Tailwind CSS** | Desarrollo rápido, sin cambiar de archivo | Clases en HTML dificultan la revisión de código; requiere configuración; `cn()` helpers adicionales |
| **Styled Components** | CSS-in-JS, dinámico | Overhead de runtime, hidratación más lenta en SSR |

**Decisión:** CSS Modules con sistema de tokens globales en `globals.css`.

**Justificación:** Para una prueba técnica que será revisada en código, la legibilidad importa. CSS Modules mantiene los estilos separados del JSX. Las variables CSS globales (`--primary`, `--bg-surface`, `--text-muted`, etc.) en `globals.css` crean un sistema de diseño coherente reutilizable en todos los módulos sin dependencias adicionales.

**Impacto en el código:** `frontend/app/globals.css` define el sistema de tokens. Cada componente tiene su `.module.css` paralelo.

---

## Decisión 3 — Almacenamiento del token JWT: localStorage

### Alternativas consideradas

| Opción | Seguridad | Complejidad | Adecuada para prueba |
|:---|:---|:---|:---|
| `localStorage` | Vulnerable a XSS | Mínima | ✅ |
| Cookies HTTP-only | Segura ante XSS | Alta (CORS cross-origin, SameSite, CSRF token) | ❌ (over-engineering) |
| Estado en memoria (React context) | Media | Media | Requiere refreshes persistentes |

**Decisión:** `localStorage` con utilidades en `lib/auth.ts`.

**Justificación:** Para el alcance de esta prueba (demostración de funcionalidad, no hardening de seguridad), `localStorage` es suficiente. Implementar cookies HTTP-only cross-origin requeriría configuración adicional de CORS (`credentials: 'include'`, `SameSite`, `Secure`) que agrega complejidad sin valor demostrable dentro del tiempo disponible.

**Nota:** En un sistema de producción, se migraría a cookies HTTP-only con refresh tokens.

---

## Decisión 4 — Cliente HTTP: fetch nativo con wrapper tipado

### Alternativas consideradas

| Opción | Pros | Contras |
|:---|:---|:---|
| `fetch` nativo con wrapper | Sin dependencias, tipado con TypeScript | Más código boilerplate |
| `axios` | Interceptores, mejor manejo de errores | Dependencia externa innecesaria para este alcance |

**Decisión:** `fetch` nativo encapsulado en `lib/api.ts`.

**Justificación:** `lib/api.ts` centraliza el token de autorización, la URL base y el manejo de errores HTTP. TypeScript garantiza que todas las funciones tienen tipado correcto (tipos definidos en `types/index.ts` alineados con los DTOs del backend). Agregar `axios` sería introducir una dependencia para funcionalidad que `fetch` cubre completamente.

---

## Consecuencias

- El frontend requiere que el backend esté corriendo en `http://localhost:5000` (configurado en `.env.local`)
- El token expira en 1 hora (configurable por `JWT_EXPIRY_HOURS` en el backend)
- Sin token válido, cualquier página del grupo `(protected)` redirige a `/login`

## Validación

Aplicación funcional en `http://localhost:3000` con flujo completo: registro → login → dashboard → crear tarea → filtrar → editar → eliminar → cerrar sesión.
