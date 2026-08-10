# ADR-003 — Base de datos y estrategia de migraciones

**Fecha:** 2026-08-09
**Estado:** Aceptado
**Originado por:** Requisito de la prueba: "base de datos relacional con scripts de creación o migraciones versionadas"

## Contexto

La prueba exige una base de datos relacional y migraciones versionadas. Se debe elegir el proveedor de BD, la estrategia de gestión del esquema y la herramienta de acceso a datos.

## Alternativas consideradas

| Aspecto | Opción A | Opción B | Elegida |
|:---|:---|:---|:---:|
| Base de datos | SQL Server 2022 | PostgreSQL | SQL Server |
| Acceso a datos | EF Core (ORM completo) | Dapper (micro-ORM) | EF Core |
| Migraciones | EF Core Migrations (C#) | Scripts SQL manuales | EF Core Migrations |
| Entorno de BD | Docker (SQL Server Express) | Instalación local | Docker |

### Por qué EF Core sobre Dapper

Dapper ofrece mayor control sobre el SQL generado y mejor rendimiento en queries complejas. Sin embargo, para este proyecto EF Core es superior por tres razones:

1. **Migraciones automáticas:** EF Core genera y aplica las migraciones como archivos C# versionados en Git. Con Dapper, las migraciones serían scripts SQL manuales propensos a inconsistencias entre entornos.
2. **Mockeable en tests:** EF Core con el patrón Repository (`IUserRepository`, `ITaskRepository`) permite inyectar mocks en los unit tests sin necesidad de una BD real. Dapper hace las queries directamente en las clases, lo que dificulta el mocking.
3. **Alineación con el stack de Jiro:** Si el equipo usa .NET y SQL Server, es probable que también use EF Core como ORM estándar.

## Decisión

**SQL Server 2022 Express via Docker + Entity Framework Core con migraciones C# versionadas + patrón Repository**

## Implementación

### Esquema de la base de datos

La migración `InitialCreate` creó las siguientes tablas:

**Tabla `Users`**
| Columna | Tipo SQL | Constraint |
|:---|:---|:---|
| `Id` | `int` | PK, IDENTITY |
| `Name` | `nvarchar(200)` | NOT NULL |
| `Email` | `nvarchar(256)` | NOT NULL, UNIQUE INDEX |
| `PasswordHash` | `nvarchar(max)` | NOT NULL |
| `CreatedAt` | `datetime2` | NOT NULL, DEFAULT GETUTCDATE |

**Tabla `Tasks`**
| Columna | Tipo SQL | Constraint |
|:---|:---|:---|
| `Id` | `int` | PK, IDENTITY |
| `Title` | `nvarchar(200)` | NOT NULL |
| `Description` | `nvarchar(2000)` | NULL (opcional) |
| `Status` | `nvarchar(20)` | NOT NULL — valores: `Pending`, `InProgress`, `Done` |
| `Priority` | `nvarchar(10)` | NOT NULL — valores: `Low`, `Medium`, `High` |
| `Deadline` | `datetime2` | NULL (opcional) |
| `CreatedAt` | `datetime2` | NOT NULL |
| `UpdatedAt` | `datetime2` | NOT NULL |
| `CreatedById` | `int` | FK → Users.Id, NOT NULL |
| `AssignedToId` | `int` | FK → Users.Id, NULL (tarea sin responsable asignado) |

### Enums como strings (no como enteros)

EF Core por defecto guarda los enums como enteros (`0`, `1`, `2`). Se configuró `HasConversion<string>()` en `AppDbContext.OnModelCreating` para que se almacenen como texto:

```csharp
entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
entity.Property(e => e.Priority).HasConversion<string>().HasMaxLength(10);
```

**Por qué:** El almacenamiento como entero rompe datos existentes si el enum se reordena en el futuro. El almacenamiento como string es autodescriptivo y permite hacer queries SQL manuales sin necesidad de un diccionario de referencia.

### Nomenclatura: `TaskItem` vs `Task`

La entidad se llama `TaskItem` (no `Task`) porque `Task` es una clase del sistema en C# (`System.Threading.Tasks.Task`) usada para programación asíncrona. Nombrar la entidad `Task` crea ambigüedad de tipos en los métodos `async`. La tabla en SQL Server se llama `Tasks` (configurada explícitamente), sin impacto en la API.

### Patrón Repository

El acceso a la BD se aísla en interfaces (`IUserRepository`, `ITaskRepository`) con implementaciones concretas (`UserRepository`, `TaskRepository`). Los servicios de negocio dependen de las interfaces, no de `AppDbContext` directamente.

**Beneficio principal:** Los unit tests pueden inyectar mocks (`Mock<IUserRepository>`) sin necesidad de BD real ni Docker.

### Entorno de testing

En entorno `Testing`, EF Core usa `UseInMemoryDatabase` en lugar de SQL Server. Esto permite correr los tests sin Docker:

```csharp
if (builder.Environment.IsEnvironment("Testing"))
    services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("TestDb"));
else
    services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));
```

## Consecuencias

- Las migraciones están bajo `backend/src/Migrations/` y se versionan con Git
- Cualquier desarrollador puede reproducir el esquema exacto con `dotnet ef database update --project backend/src`
- EF Core genera queries parametrizadas → protección automática contra SQL injection
- Los tests no requieren Docker ni SQL Server corriendo

## Validación

Migración `InitialCreate` aplicada exitosamente. SQL Server creó las tablas con las constraints, índices y tipos de datos correctos.

```bash
dotnet ef database update --project backend/src
# Output: Applying migration '20260809XXXXXX_InitialCreate'. Done.
```
