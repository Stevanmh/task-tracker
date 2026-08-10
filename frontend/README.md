# Task Tracker — Frontend

Interfaz web del sistema de gestión de tareas. Construida con **Next.js 16 (App Router)**, consume la API REST del backend (.NET 10) en `http://localhost:5000`.

---

## Stack

| Tecnología           | Versión | Rol                                 |
| :------------------- | :------ | :---------------------------------- |
| Next.js (App Router) | 16.3.0  | Framework de frontend               |
| TypeScript           | 5.x     | Tipado estático                     |
| CSS Modules          | —       | Estilos encapsulados por componente |
| Inter (Google Fonts) | —       | Tipografía del sistema de diseño    |

---

## Requisitos previos

- Node.js 20+
- El backend corriendo en `http://localhost:5000`
- Archivo `frontend/.env.local` con `NEXT_PUBLIC_API_URL=http://localhost:5000`

---

## Instalación y ejecución

```bash
# Instalar dependencias
npm install

# Iniciar servidor de desarrollo
npm run dev
```

La app estará disponible en `http://localhost:3000`.

---

## Estructura del proyecto

```
frontend/
├── app/
│   ├── globals.css              ← Sistema de diseño: tokens CSS, dark mode
│   ├── layout.tsx               ← Root layout con metadatos SEO
│   ├── page.tsx                 ← Redirige a /dashboard o /login según token
│   ├── (protected)/             ← Grupo de rutas privadas (auth guard)
│   │   ├── layout.tsx           ← Verifica token, redirige a /login si no hay
│   │   ├── dashboard/page.tsx   ← Métricas: total tareas por estado
│   │   └── tasks/page.tsx       ← CRUD de tareas + filtros dinámicos
│   ├── login/page.tsx           ← Formulario de inicio de sesión
│   └── register/page.tsx        ← Formulario de registro
├── components/
│   ├── Navbar.tsx               ← Navegación con active state y logout
│   └── TaskModal.tsx            ← Modal crear/editar tarea (cierre con Escape)
├── lib/
│   ├── api.ts                   ← Cliente HTTP tipado (fetch + Bearer token automático)
│   └── auth.ts                  ← Lectura/escritura del JWT en localStorage
└── types/
    └── index.ts                 ← Tipos TypeScript: TaskItem, User, AuthResponse, etc.
```

---

## Páginas y funcionalidades

| Ruta         | Acceso  | Descripción                                                          |
| :----------- | :-----: | :------------------------------------------------------------------- |
| `/login`     | Público | Formulario de login. Redirige a `/dashboard` si ya hay sesión        |
| `/register`  | Público | Formulario de registro de nuevo usuario                              |
| `/dashboard` | 🔒 Auth | Tarjetas con métricas: Pendientes, En Progreso, Completadas, Total   |
| `/tasks`     | 🔒 Auth | Lista de tareas con filtros, modal de creación/edición y eliminación |

### Gestión de tareas (`/tasks`)

- **Crear:** Botón "Nueva Tarea" → abre modal con formulario
- **Editar:** Botón de edición en cada tarea → abre modal precargado
- **Eliminar:** Botón de eliminar con confirmación
- **Filtrar:** Dropdowns de Estado, Prioridad y Responsable (combinables)

---

## Variables de entorno

| Variable              | Dónde                 | Descripción                                       |
| :-------------------- | :-------------------- | :------------------------------------------------ |
| `NEXT_PUBLIC_API_URL` | `frontend/.env.local` | URL base del backend. Ej: `http://localhost:5000` |

> El prefijo `NEXT_PUBLIC_` es requerido para que Next.js exponga la variable al cliente (navegador).

---

## Sistema de diseño

El diseño usa **dark mode** por defecto con variables CSS globales definidas en `app/globals.css`:

| Token            | Valor por defecto | Uso                            |
| :--------------- | :---------------- | :----------------------------- |
| `--primary`      | `#6c63ff`         | Botones, acentos, active state |
| `--bg-base`      | `#0f0f13`         | Fondo principal                |
| `--bg-surface`   | `#1a1a24`         | Tarjetas, modales              |
| `--bg-elevated`  | `#22223a`         | Inputs, hover states           |
| `--text-primary` | `#f0f0f5`         | Texto principal                |
| `--text-muted`   | `#8888aa`         | Labels, texto secundario       |

Tipografía: **Inter** (Google Fonts), cargada con `next/font/google`.

---

## Conexión con el backend

Todas las requests HTTP pasan por `lib/api.ts`, que:

1. Lee `NEXT_PUBLIC_API_URL` como base URL
2. Agrega el header `Authorization: Bearer <token>` automáticamente en cada request (leyendo de `localStorage`)
3. Maneja errores HTTP y los propaga como excepciones tipadas

El token se almacena y lee desde `localStorage` vía `lib/auth.ts`.
