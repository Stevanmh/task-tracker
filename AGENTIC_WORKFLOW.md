# AGENTIC_WORKFLOW.md

Registro del proceso de desarrollo con asistencia de IA (**AntiGravity IDE**) para la prueba técnica de Jiro/Accenture.

---

## Propósito

Este documento evidencia cómo se utilizó la IA como herramienta de aceleración — no como sustituto del criterio técnico. Para cada interacción relevante se documenta:

**Prompt/instrucción → Propuesta del agente → Revisión humana → Decisión → Implementación → Validación**

Solo se registran las interacciones donde la decisión técnica tuvo impacto observable en el código. No es un historial de conversación.

---

## Herramientas utilizadas

| Herramienta | Modelo subyacente | Para qué se usó |
|:---|:---|:---|
| **AntiGravity IDE** | Google Gemini | Arquitectura, generación de código, decisiones técnicas, debugging, documentación, tests |

> La documentación técnica (architecture.md, api.md, setup.md, ADRs, README) fue generada con asistencia del agente y revisada/corregida por el candidato para garantizar que refleje el código real.

---

## Metodología adoptada

```
1. Instrucción precisa al agente (sin delegar la decisión)
2. El agente propone una implementación
3. Revisión crítica: ¿resuelve el problema correctamente? ¿está alineada con las constraints del proyecto?
4. Decisión: Aceptar / Modificar / Rechazar
5. Implementación (con o sin ajustes)
6. Validación: compilación, ejecución, tests
```

El agente no tomó decisiones arquitectónicas de forma autónoma. Cada propuesta fue evaluada antes de aplicarse.

---

## Registro de interacciones

---

### [FEAT-01] — Stack tecnológico

**Prompt dado al agente:**
> "El proyecto va a usar Next.js en el frontend, .NET Web API en el backend, SQL Server en Docker y Entity Framework Core como ORM. Genera la estructura inicial del proyecto."

**Propuesta del agente:**
Next.js 14 + .NET 9 Web API + SQL Server 2022 Express (Docker) + EF Core.

**Revisión humana:**
El stack es correcto pero las versiones propuestas están desactualizadas respecto al entorno real:
- `.NET 9` → la máquina tiene `.NET 10` instalado. No hay razón para instalar una versión anterior.
- `Next.js 14` → `create-next-app` instala `16.3.0`. Se usa la versión disponible.

**Decisión:** 🔄 Modificado — versiones ajustadas a lo instalado en la máquina

**Implementación:**
- `backend/src/TaskTracker.Api.csproj` → `<TargetFramework>net10.0</TargetFramework>`
- `docker-compose.yml` → `mcr.microsoft.com/mssql/server:2022-express-ubuntu22.04`

**Validación:** `dotnet --version` → `10.0.204` ✅ | `node --version` → `v24.14.0` ✅



---

### [FEAT-02] — Carga del archivo `.env` en .NET

**Prompt dado al agente:**
> "El backend debe leer las variables de entorno desde el `.env` en la raíz del proyecto."

**Propuesta del agente:**
Usar `DotNetEnv` con `Env.TraversePath().Load()`.

**Revisión humana:**
La alternativa sería una ruta relativa hardcodeada (`../../.env`). El problema: esa ruta cambia según desde dónde se ejecute el comando. `TraversePath()` sube por el árbol de directorios hasta encontrar el `.env`, sin importar el directorio de trabajo — más robusto.

**Decisión:** ✅ Aceptado

**Implementación:** Primera línea de `Program.cs` → `Env.TraversePath().Load();`

**Validación:** `dotnet ef database update --project backend/src` ejecutado desde la raíz del proyecto completó correctamente, confirmando que el `.env` fue encontrado y leído.

---

### [FIX-01] — Swagger no compila en .NET 10

**Prompt dado al agente:**
> "Configura Swagger/OpenAPI para que la API tenga documentación interactiva."

**Propuesta del agente:**
`Program.cs` con `AddSwaggerGen()`, `UseSwagger()` y `UseSwaggerUI()`.

**Error detectado:**
```
error CS1061: 'IServiceCollection' does not contain a definition for 'AddSwaggerGen'
```

**Causa:** El agente asumió que Swashbuckle estaba incluido en la plantilla `dotnet new webapi` de .NET 10. No lo está — .NET 10 usa su propio `Microsoft.AspNetCore.OpenApi` con una API diferente.

**Revisión humana:**
El proveedor nativo de .NET 10 no incluye UI interactiva por defecto. Para la demostración de la prueba, la UI de Swagger (`/swagger`) es necesaria. Se instaló Swashbuckle explícitamente.

**Decisión:** 🔄 Modificado — instalar paquete faltante

```bash
dotnet add backend/src package Swashbuckle.AspNetCore
```

**Validación:** `dotnet build backend/src` → compilación exitosa. UI disponible en `http://localhost:5000/swagger` ✅

---

### [FEAT-03] — Estrategia de tests: unit + integración

**Prompt dado al agente:**
> "Necesitamos dos capas de tests: unitarios sobre `AuthService` (que validen la lógica de negocio sin tocar la BD) y de integración sobre los endpoints HTTP principales. Usa xUnit. Para los unitarios, mockea `IUserRepository` con Moq. Para los de integración, levanta el servidor completo en memoria."

**Propuesta del agente:**
Dos capas de tests con xUnit:
- Unit tests sobre `AuthService` usando `Mock<IUserRepository>` (sin BD real)
- Integration tests sobre los endpoints HTTP usando `WebApplicationFactory<Program>` con BD InMemory

**Revisión humana:**
La estrategia es correcta. El patrón Repository (interfaces `IUserRepository`, `ITaskRepository`) permite mockear correctamente sin EF Core. `WebApplicationFactory` levanta el servidor completo en memoria.

**Decisión:** ✅ Aceptado

**Implementación:** `backend/tests/Unit/AuthServiceTests.cs` + `backend/tests/Integration/AuthEndpointsTests.cs`

**Validación (primera ejecución):**
Los tests de integración fallaban — todos devolvían `409 Conflict` en lugar de los códigos esperados (`201`, `200`, `401`). Ver [FIX-02].

---

### [FIX-02] — Conflicto de proveedores de EF Core en tests de integración

**Contexto:** Los tests de integración fallaban con `409 Conflict` en todos los endpoints que tocaban la BD.

**Primera propuesta del agente (rechazada):**
Manipular el DI container en `TestWebApplicationFactory` — remover el descriptor de `DbContextOptions<AppDbContext>` y registrar uno nuevo con InMemory:

```csharp
// v1 — RECHAZADA
builder.ConfigureServices(services => {
    var descriptor = services.FirstOrDefault(
        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
    services.Remove(descriptor);
    services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("TestDb"));
});
```

**Por qué se rechazó:**
El resultado fue el mismo error. EF Core 7+ mantiene un "internal service provider" propio que ya tenía SQL Server registrado — remover el descriptor de ASP.NET DI no limpia el proveedor interno de EF Core. El error `"Only a single database provider can be registered"` es de EF Core, no de ASP.NET.

**Segunda propuesta del agente (aceptada):**
Detección de entorno directamente en `Program.cs`. Si el entorno es `"Testing"`, registrar InMemory antes de que cualquier servicio quede registrado:

```csharp
// v2 — ACEPTADA
if (builder.Environment.IsEnvironment("Testing"))
    builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("TaskTrackerTestDb"));
else
    builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));
```

La factory solo hace `builder.UseEnvironment("Testing")`.

**Decisión:** ❌ v1 Rechazado → ✅ v2 Aceptado

**Validación:**
```
Pruebas totales: 10 | Correcto: 10 ✅
```



---

### [FIX-03] — Puerto del backend desalineado

**Contexto:** Con ambos servicios corriendo, el frontend no conectaba al backend.

**Diagnóstico:**
`dotnet new webapi` asignó el puerto `5162` en `launchSettings.json`. El frontend estaba configurado para apuntar a `http://localhost:5000`.

**Propuesta del agente:**
Cambiar `launchSettings.json` para usar `5000`.

**Alternativa considerada:**
Actualizar `frontend/.env.local` para apuntar a `5162`. Rechazada — `5000` es el puerto convencional para APIs .NET en desarrollo. Cambiar el `.env.local` habría requerido actualizar todas las referencias futuras.

**Decisión:** ✅ Aceptado — cambiar el puerto en el backend, no en el frontend

**Implementación:** `backend/src/Properties/launchSettings.json` → `"applicationUrl": "http://localhost:5000"`

**Validación:** El frontend conectó correctamente al backend ✅

---

### [FEAT-04] — Protección de rutas en el frontend (auth guard)

**Prompt dado al agente:**
> "Las rutas /dashboard y /tasks deben ser privadas — solo accesibles con token válido. ¿Qué opciones hay en Next.js App Router?"

**Propuesta del agente — tres opciones:**

| Opción | Cómo funciona | Limitación |
|:---|:---|:---|
| `middleware.ts` (servidor) | Intercepta cada request antes de renderizar | No puede leer `localStorage` — el JWT está en el cliente |
| Layout de grupo `(protected)/layout.tsx` | Verifica el token en `useEffect` y redirige | Breve flash de contenido durante la hidratación |
| HOC por página | Envuelve cada página individualmente | Lógica repetida en cada ruta protegida |

**Revisión humana:**
El `middleware.ts` sería la opción más robusta en producción (no hay flash), pero requiere que el token esté en cookies HTTP-only — incompatible con la estrategia de `localStorage` ya decidida. El HOC introduce código repetido. El layout de grupo centraliza toda la lógica de auth en un solo archivo y todas las páginas del grupo lo heredan.

**Decisión:** ✅ Layout de grupo `(protected)/layout.tsx` — con tradeoff documentado

**Tradeoff aceptado:** Flash de contenido breve antes de redirigir. Aceptable para el alcance de la prueba; en producción se migraría a cookies HTTP-only + `middleware.ts`.

**Implementación:** `frontend/app/(protected)/layout.tsx`

**Validación:** Acceder a `/dashboard` sin token redirige a `/login` ✅



---

### [FEAT-05] — Linting y formateo de código

**Prompt dado al agente:**
> "Configura linting y formateo de código para el frontend y el backend."

**Propuesta del agente:**
- Frontend: ESLint con `eslint-config-next` + `@typescript-eslint/recommended` + Prettier
- Backend: `.editorconfig` global + `dotnet format` (integrado en el SDK de .NET)

**Error detectado durante la implementación:**
```
npm error notarget No matching version found for @typescript-eslint/scope-manager@8.67.0
```
`eslint-config-next` dependía de una versión de `@typescript-eslint` que no existía en el registry al momento del desarrollo (incompatibilidad con Next.js 16 recién publicado).

**Revisión humana:**
Se descartó `eslint-config-next` y se usó `@typescript-eslint@7` directamente — versión estable, sin conflictos de peer dependencies.

**Decisión:** 🔄 Modificado — `eslint-config-next` → `@typescript-eslint@7` por conflicto de dependencias

**Implementación:**
- `frontend/.eslintrc.json` — ESLint con TypeScript y Prettier
- `frontend/.prettierrc` — reglas de formateo (single quotes, 100 chars, trailing comma)
- `frontend/package.json` — scripts `lint`, `lint:fix`, `format`, `format:check`
- `.editorconfig` — raíz del proyecto, aplica a C# y JSON del backend

**Validación:**
```bash
npm run lint   # 0 errores ✅
npm run format # 15 archivos formateados ✅
```

---

## Resumen de decisiones

| ID | Tipo | Título | Decisión |
|:---|:---:|:---|:---:|
| FEAT-01 | Feature | Stack tecnológico | 🔄 Modificado |
| FEAT-02 | Feature | Carga de `.env` con DotNetEnv | ✅ Aceptado |
| FIX-01 | Fix | Swashbuckle asumido como instalado en .NET 10 | 🔧 Error corregido |
| FEAT-03 | Feature | Estrategia de tests (unit + integración) | ✅ Aceptado |
| FIX-02 | Fix | Conflicto de proveedores EF Core en tests | ❌→✅ Rechazado v1 / Aceptado v2 |
| FIX-03 | Fix | Puerto del backend desalineado (5162→5000) | ✅ Aceptado |
| FEAT-04 | Feature | Auth guard en Next.js App Router | ✅ Aceptado (con tradeoff) |
| FEAT-05 | Feature | Linting y formateo (ESLint + Prettier + EditorConfig) | 🔄 Modificado |
