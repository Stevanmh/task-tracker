# ADR-004 — Estrategia de testing

**Fecha:** 2026-08-09
**Estado:** Aceptado
**Originado por:** Requisito de la prueba: "pruebas automatizadas — al menos unitarias sobre lógica de negocio del backend"

## Contexto

La prueba evalúa testing con un peso del 15%. Se debe definir qué se prueba, con qué herramienta y hasta qué nivel de cobertura es viable dentro del tiempo disponible.

## Alternativas consideradas

| Tipo | Herramienta | Alcance |
|:---|:---|:---|
| **Unit tests** | xUnit + Moq | Lógica de servicios aislada (sin BD) |
| **Integration tests** | xUnit + WebApplicationFactory | Endpoints HTTP reales contra BD en memoria |
| **E2E tests** | Playwright | Flujo completo UI → API → BD |

## Decisión

**xUnit para unit tests + xUnit con WebApplicationFactory para integration tests**

E2E tests descartados por restricción de tiempo.

## Qué se implementó

### Unit tests — `AuthServiceTests.cs` (6 tests)
Prueban `AuthService` aislado con `Mock<IUserRepository>` (sin BD real):

| Test | Verifica |
|:---|:---|
| `Register_WithValidData_ReturnsAuthResponseWithToken` | Registro exitoso devuelve token |
| `Register_WithDuplicateEmail_ThrowsInvalidOperationException` | Email duplicado lanza excepción de negocio |
| `Register_PasswordNotStoredInPlainText` | BCrypt hashea la contraseña (no se guarda texto plano) |
| `Login_WithCorrectCredentials_ReturnsAuthResponseWithToken` | Login correcto devuelve token válido |
| `Login_WithWrongPassword_ThrowsUnauthorizedAccessException` | Contraseña incorrecta → excepción de auth |
| `Login_WithNonExistentEmail_ThrowsUnauthorizedAccessException` | Email inexistente → excepción de auth |

### Integration tests — `AuthEndpointsTests.cs` (5 tests)
Prueban el flujo HTTP completo con `WebApplicationFactory<Program>` + BD InMemory:

| Test | Endpoint | Resultado esperado |
|:---|:---|:---|
| `Register_WithValidData_Returns201Created` | `POST /api/auth/register` | `201 Created` + token |
| `Login_WithValidCredentials_Returns200WithToken` | `POST /api/auth/login` | `200 OK` + token |
| `Login_WithInvalidCredentials_Returns401` | `POST /api/auth/login` (credenciales malas) | `401 Unauthorized` |
| `GetTasks_WithoutToken_Returns401Unauthorized` | `GET /api/tasks` (sin header) | `401 Unauthorized` |

### Nota técnica — Resolución del entorno de testing
La BD InMemory se activa condicionalmente en `Program.cs`:
```csharp
if (builder.Environment.IsEnvironment("Testing"))
    services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("TestDb"));
else
    services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));
```
Esto evita el conflicto de "múltiples proveedores de BD" que se produce cuando se intenta reemplazar descriptores de EF Core en el DI container después del registro inicial.

## Consecuencias

- Tests corren sin Docker ni SQL Server: `dotnet test backend/tests`
- El backend no puede estar corriendo cuando se ejecutan los tests (el exe está bloqueado)
- `Microsoft.EntityFrameworkCore.InMemory` se instaló también en el proyecto API (no solo en tests)

## Validación

```
Pruebas totales: 10; con errores: 0; correcto: 10
```

> Nota: se eliminó el archivo `UnitTest1.cs` (placeholder vacío generado por `dotnet new xunit` sin assertions). Los 10 tests son todos con propósito explícito.

