# Referencia de la API — Task Tracker

## Base URL

```
http://localhost:5000/api
```

## Autenticación

Todos los endpoints excepto `/auth/register` y `/auth/login` requieren el header:

```
Authorization: Bearer <token>
```

El token JWT se obtiene al registrarse o iniciar sesión. Expira según `JWT_EXPIRY_HOURS` del `.env`.

---

## Endpoints

### Auth

#### `POST /api/auth/register`
Registra un nuevo usuario y devuelve un JWT.

**Body:**
```json
{
  "name": "Carlos",
  "email": "carlos@example.com",
  "password": "Test1234!"
}
```

**Respuestas:**

| Código | Descripción |
|:---|:---|
| `201 Created` | Usuario creado. Devuelve `AuthResponse` |
| `400 Bad Request` | Datos inválidos |
| `409 Conflict` | El email ya está registrado |

**Response body (201):**
```json
{
  "id": 1,
  "name": "Carlos",
  "email": "carlos@example.com",
  "token": "eyJhbGci..."
}
```

---

#### `POST /api/auth/login`
Inicia sesión y devuelve un JWT.

**Body:**
```json
{
  "email": "carlos@example.com",
  "password": "Test1234!"
}
```

**Respuestas:**

| Código | Descripción |
|:---|:---|
| `200 OK` | Login exitoso. Devuelve `AuthResponse` |
| `401 Unauthorized` | Credenciales incorrectas |

**Response body (200):**
```json
{
  "id": 1,
  "name": "Carlos",
  "email": "carlos@example.com",
  "token": "eyJhbGci..."
}
```

---

### Tasks

#### `GET /api/tasks`
Lista todas las tareas. Soporta filtros opcionales por query string.

**Query params (todos opcionales):**

| Param | Tipo | Valores |
|:---|:---|:---|
| `status` | string | `Pending`, `InProgress`, `Done` |
| `priority` | string | `Low`, `Medium`, `High` |
| `assignedToId` | int | ID del usuario responsable |

**Ejemplo:** `GET /api/tasks?status=Pending&priority=High`

**Response (200):**
```json
[
  {
    "id": 1,
    "title": "Implementar login",
    "description": "JWT con BCrypt",
    "status": "InProgress",
    "priority": "High",
    "deadline": "2026-08-15T00:00:00",
    "createdAt": "2026-08-10T00:00:00",
    "updatedAt": "2026-08-10T00:00:00",
    "createdBy": { "id": 1, "name": "Carlos", "email": "carlos@example.com" },
    "assignedTo": { "id": 1, "name": "Carlos", "email": "carlos@example.com" }
  }
]
```

---

#### `POST /api/tasks`
Crea una nueva tarea.

**Body:**
```json
{
  "title": "Implementar login",
  "description": "JWT con BCrypt",
  "status": "Pending",
  "priority": "High",
  "assignedToId": 1,
  "deadline": "2026-08-15"
}
```

| Campo | Requerido | Descripción |
|:---|:---:|:---|
| `title` | ✅ | Máx. 200 caracteres |
| `description` | ❌ | Máx. 2000 caracteres |
| `status` | ✅ | `Pending` \| `InProgress` \| `Done` |
| `priority` | ✅ | `Low` \| `Medium` \| `High` |
| `assignedToId` | ❌ | ID de usuario existente |
| `deadline` | ❌ | Fecha ISO 8601 |

**Respuestas:** `201 Created` / `400 Bad Request` / `401 Unauthorized`

---

#### `GET /api/tasks/{id}`
Obtiene una tarea por su ID.

**Respuestas:** `200 OK` / `401 Unauthorized` / `404 Not Found`

**Response body (200):**
```json
{
  "id": 1,
  "title": "Implementar login",
  "description": "JWT con BCrypt",
  "status": "InProgress",
  "priority": "High",
  "deadline": "2026-08-15T00:00:00",
  "createdAt": "2026-08-10T00:00:00",
  "updatedAt": "2026-08-10T00:00:00",
  "createdBy": { "id": 1, "name": "Carlos", "email": "carlos@example.com" },
  "assignedTo": { "id": 1, "name": "Carlos", "email": "carlos@example.com" }
}
```

---

#### `PUT /api/tasks/{id}`
Actualiza una tarea existente.

**Body:** misma estructura que `POST /api/tasks` (todos los campos).

**Respuestas:** `200 OK` / `400 Bad Request` / `401 Unauthorized` / `404 Not Found`

**Response body (200):** misma estructura que `GET /api/tasks/{id}`.

---

#### `DELETE /api/tasks/{id}`
Elimina una tarea permanentemente (hard delete).

**Respuestas:** `204 No Content` / `401 Unauthorized` / `404 Not Found`

---

### Dashboard

#### `GET /api/dashboard`
Devuelve métricas del estado actual de las tareas.

**Response (200):**
```json
{
  "totalTasks": 10,
  "pendingTasks": 4,
  "inProgressTasks": 3,
  "doneTasks": 3
}
```

---

### Users

#### `GET /api/users`
Lista todos los usuarios registrados. Usado por el frontend para poblar el selector de responsable.

**Response (200):**
```json
[
  { "id": 1, "name": "Carlos", "email": "carlos@example.com" }
]
```

---

## Modelos de datos

### Enums

```
TaskStatus : Pending | InProgress | Done
TaskPriority: Low | Medium | High
```

### Manejo de errores

Todos los errores siguen el formato:

```json
{ "message": "Descripción del error" }
```

| HTTP | Causa |
|:---|:---|
| `400` | Datos de entrada inválidos |
| `401` | Sin token o credenciales incorrectas |
| `404` | Recurso no encontrado |
| `409` | Conflicto (ej: email duplicado) |
| `500` | Error interno del servidor |

---

## Tests automatizados

**Suite:** `TaskTracker.Tests` (xUnit + Moq + WebApplicationFactory)

| Categoría | Archivo | Tests | Resultado |
|:---|:---|:---:|:---:|
| Unit — AuthService | `Unit/AuthServiceTests.cs` | 6 | ✅ |
| Integration — Auth endpoints | `Integration/AuthEndpointsTests.cs` | 4 | ✅ |
| **Total** | | **10** | **10/10 ✅** |

**Ejecutar:**
```bash
dotnet test backend/tests
```
