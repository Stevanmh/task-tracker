# Guía de instalación y ejecución

## Prerrequisitos

| Herramienta | Versión mínima | Descarga |
|:---|:---|:---|
| Docker Desktop | 4.x | [docker.com](https://www.docker.com/products/docker-desktop/) |
| .NET SDK | 10.0 | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| Node.js | 20+ | [nodejs.org](https://nodejs.org/) |
| Git | 2.x | [git-scm.com](https://git-scm.com/) |

Verificar versiones instaladas:
```bash
dotnet --version    # debe mostrar 10.x.x
node --version      # debe mostrar v20.x.x o superior
docker --version    # debe mostrar 4.x o superior
```

---

## Instalación paso a paso

### 1. Clonar el repositorio

```bash
git clone https://github.com/Stevanmh/task-tracker.git
cd task-tracker
```

### 2. Configurar variables de entorno del backend

```bash
copy .env.example .env
```

Abrir el archivo `.env` y asignar los valores obligatorios:

```env
DB_SERVER=localhost
DB_PORT=1433
DB_NAME=TaskTrackerDb
DB_USER=sa
DB_PASSWORD=TuContraseñaSegura@123   # Mínimo 8 chars, mayúsculas + números + símbolos

JWT_SECRET=una-cadena-larga-y-aleatoria-de-minimo-32-caracteres
JWT_EXPIRY_HOURS=1

CORS_ORIGIN=http://localhost:3000
```

> El archivo `.env` va en la **raíz del proyecto** (`task-tracker/.env`), no dentro de `backend/`.

### 3. Configurar variables de entorno del frontend

El frontend necesita saber en qué URL corre el backend. Crear el archivo `frontend/.env.local`:

```bash
# En Windows:
echo NEXT_PUBLIC_API_URL=http://localhost:5000 > frontend/.env.local
```

O crearlo manualmente con el contenido:
```env
NEXT_PUBLIC_API_URL=http://localhost:5000
```

> Sin este archivo, el frontend no podrá conectarse al backend y todas las requests fallarán.

### 4. Levantar SQL Server con Docker

```bash
docker-compose up -d
```

Verificar que el contenedor esté corriendo:

```bash
docker ps
```

Debes ver un contenedor `sqlserver` o similar con estado `Up`. Espera **~30 segundos** hasta que SQL Server esté listo para aceptar conexiones antes de continuar.

### 5. Aplicar migraciones (crear tablas en la BD)

```bash
dotnet ef database update --project backend/src
```

Resultado esperado: el comando debe terminar sin errores y mostrar que la migración `InitialCreate` fue aplicada. Esto crea las tablas `Users` y `Tasks` en SQL Server.

### 6. Iniciar el backend

```bash
dotnet run --project backend/src
```

El backend estará disponible en:
- **API REST:** `http://localhost:5000/api`
- **Swagger UI:** `http://localhost:5000/swagger` (solo en Development)

### 7. Iniciar el frontend

En una **terminal separada** (el backend debe seguir corriendo):

```bash
cd frontend
npm install
npm run dev
```

La aplicación estará disponible en `http://localhost:3000`.

---

## Flujo de uso básico

1. Ir a `http://localhost:3000` → redirige a `/login`
2. Hacer click en "Registrarse" → crear cuenta
3. Iniciar sesión → ir al Dashboard con métricas
4. Ir a "Tareas" → crear, editar, eliminar y filtrar tareas

---

## Ejecutar los tests

> **Importante:** El backend **no debe estar corriendo** cuando ejecutes los tests — el exe está bloqueado por el proceso y el compilador no puede sobreescribirlo. Detenerlo con `Ctrl+C` antes de correr los tests.

Los tests usan una BD InMemory independiente — **no requieren Docker ni SQL Server corriendo**.

```bash
dotnet test backend/tests
```

Resultado esperado:
```
Pruebas totales: 10; con errores: 0; correcto: 10
```

Para ver los tests en detalle con sus nombres:
```bash
dotnet test backend/tests --verbosity normal
```

---

## Linting y formateo

### Frontend (ESLint + Prettier)

```bash
cd frontend

# Verificar errores de linting:
npm run lint

# Corregir errores corregibles automáticamente:
npm run lint:fix

# Formatear todo el código:
npm run format

# Verificar formato sin modificar archivos:
npm run format:check
```

### Backend (.NET)

```bash
# Aplicar formato según .editorconfig:
dotnet format backend/src
```

---

## Detener el entorno


```bash
# Detener SQL Server (mantiene los datos)
docker-compose down

# Detener SQL Server y eliminar los datos (volumen)
docker-compose down -v
```

---

## Solución de problemas comunes

| Problema | Causa probable | Solución |
|:---|:---|:---|
| `Connection refused` al correr el backend | SQL Server aún iniciando | Esperar 30s y reintentar |
| `Login failed for user 'sa'` | Contraseña incorrecta en `.env` | Verificar `DB_PASSWORD` en `.env` |
| `Port 1433 already in use` | SQL Server local instalado en la máquina | Cambiar `DB_PORT` en `.env` a `1434` y actualizar `docker-compose.yml` |
| `JWT_SECRET not configured` | `.env` no cargado o mal ubicado | Verificar que `.env` existe en la **raíz del proyecto**, no en `backend/` |
| Frontend muestra `ERR_CONNECTION_REFUSED` | Backend no está corriendo | Iniciar el backend con `dotnet run --project backend/src` |
| Frontend no muestra datos (respuestas vacías) | `frontend/.env.local` no existe o tiene URL incorrecta | Verificar que `NEXT_PUBLIC_API_URL=http://localhost:5000` existe en `frontend/.env.local` |
| Tests fallan con error de archivo bloqueado | El backend está corriendo | Detenerlo con `Ctrl+C` y volver a correr `dotnet test backend/tests` |
| Tests fallan con `Only a single database provider` | Versión incorrecta del código | Verificar que `Program.cs` tiene la detección de entorno `IsEnvironment("Testing")` |
