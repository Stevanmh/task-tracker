# ADR-001 — Selección de stack tecnológico

**Fecha:** 2026-08-09
**Estado:** Aceptado
**Originado por:** Iniciativa del candidato — investigación del stack tecnológico de Jiro

## Contexto

La prueba técnica no especifica el stack. Definí el stack basándome en el ecosistema donde quiero demostrar competencia: .NET 10 (backend robusto con tipado fuerte y arquitectura por capas), Next.js con App Router (frontend moderno, SSR/CSR flexible), SQL Server con EF Core (ORM maduro con migraciones versionadas). SQL Server en Docker garantiza un entorno reproducible sin dependencia de instalaciones locales.

## Alternativas consideradas

| Capa | Opción A | Opción B | Elegida |
|:---|:---|:---|:---:|
| Frontend | Next.js | Vue 3 | Next.js |
| Backend | .NET 10 Web API | Node.js + Express | .NET 10 |
| Base de datos | SQL Server (Docker) | PostgreSQL (Docker) | SQL Server |
| ORM | Entity Framework Core | Prisma / Dapper | EF Core |

## Decisión

**Next.js 16.3.0 (App Router) + .NET 10 Web API + SQL Server 2022 (Docker) + Entity Framework Core**

## Consecuencias

- El evaluador verá tecnologías que conoce en profundidad → menor fricción en la revisión
- .NET 10 requiere manejo de tipado fuerte y arquitectura en capas → mayor tiempo inicial de setup pero código más robusto
- SQL Server via Docker elimina dependencia de instalación local → entorno reproducible

## Validación

Verificado con `dotnet --version`, `node --version`, `docker --version` antes de iniciar el desarrollo.
