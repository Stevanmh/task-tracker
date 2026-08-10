# ADR-002 — Estrategia de autenticación: JWT stateless

**Fecha:** 2026-08-09
**Estado:** Aceptado
**Originado por:** Requisito funcional de la prueba: "autenticación básica de usuarios"

## Contexto

La prueba requiere autenticación. La arquitectura tiene el frontend (Next.js, puerto 3000) y el backend (.NET Web API, puerto 5000) como aplicaciones completamente separadas que se comunican por HTTP. Se debe elegir un mecanismo de autenticación compatible con APIs REST cross-origin.

## Alternativas consideradas

| Opción | Pros | Contras |
|:---|:---|:---|
| **JWT (stateless)** | No requiere estado en servidor, escala horizontalmente, estándar para APIs REST consumidas por SPA | Token no puede invalidarse antes de expirar sin infraestructura adicional (blacklist) |
| **Sessions (stateful)** | Invalidación inmediata posible | Requiere almacenamiento de sesión en servidor; configuración de cookies cross-origin compleja (`SameSite`, CORS credentials) |

## Decisión

**JWT (JSON Web Token) stateless** firmado con HMAC-SHA256, expiración configurable por `JWT_EXPIRY_HOURS` (por defecto: 1 hora).

## Implementación

### Generación del token (`AuthService.GenerateJwt`)

El token incluye los siguientes claims:

| Claim | Tipo | Valor |
|:---|:---|:---|
| `NameIdentifier` | `ClaimTypes.NameIdentifier` | ID del usuario (entero → string) |
| `Email` | `ClaimTypes.Email` | Email normalizado en lowercase |
| `Name` | `ClaimTypes.Name` | Nombre del usuario |

Algoritmo de firma: **HMAC-SHA256** (`SecurityAlgorithms.HmacSha256`).
Clave de firma: derivada de `JWT_SECRET` del `.env` (mínimo recomendado: 32 caracteres).

### Hash de contraseñas (BCrypt)

Las contraseñas **nunca se almacenan en texto plano**. Se usa `BCrypt.Net.BCrypt.HashPassword()` al registrar y `BCrypt.Net.BCrypt.Verify()` al hacer login.

BCrypt incluye un salt aleatorio por defecto — cada hash de la misma contraseña es diferente, lo que protege contra ataques de rainbow tables.

### Protección contra user enumeration (timing-safe login)

```csharp
var user = await _userRepository.GetByEmailAsync(request.Email.ToLowerInvariant());

if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
    throw new UnauthorizedAccessException("Credenciales inválidas");
```

Si el email no existe **o** la contraseña es incorrecta, se lanza la misma excepción con el mismo mensaje. Esto previene que un atacante pueda distinguir entre "email no registrado" y "contraseña incorrecta" — protección contra *user enumeration attacks*.

### Normalización de email

Los emails se convierten a lowercase con `ToLowerInvariant()` tanto al registrar como al buscar. Esto evita duplicados por capitalización (`Usuario@Test.com` y `usuario@test.com` se tratan como el mismo usuario).

### Almacenamiento del token en el cliente

El token se guarda en `localStorage` del navegador (`lib/auth.ts`). En cada request, `lib/api.ts` lo recupera y lo agrega al header `Authorization: Bearer <token>` automáticamente.

> **Nota de seguridad:** `localStorage` es vulnerable a ataques XSS. En un sistema de producción se migraría a cookies HTTP-only con refresh tokens. Para el alcance de esta prueba técnica, `localStorage` es suficiente.

## Consecuencias

- Las rutas protegidas tienen el atributo `[Authorize]` en sus controllers
- `JwtBearer` middleware de ASP.NET Core valida la firma y la expiración automáticamente
- Si el token expira o es inválido → `401 Unauthorized` automático (sin código adicional en los controllers)
- El frontend redirige a `/login` si no encuentra token en `localStorage`

## Validación

Implementado con `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.10.

Tests de integración que validan el comportamiento:
- `Login_WithValidCredentials_Returns200WithToken` → el token generado es no vacío
- `Login_WithInvalidCredentials_Returns401` → credenciales incorrectas devuelven 401
- `GetTasks_WithoutToken_Returns401Unauthorized` → sin token el endpoint protegido devuelve 401
