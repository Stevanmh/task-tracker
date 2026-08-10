# Arquitectura del Sistema

> Documento en construcción — se completa al finalizar el desarrollo.

## Diagrama de capas

```mermaid
graph TD
    subgraph Frontend ["Frontend — Next.js 14"]
        UI[Pages / Components]
        APIClient[API Client lib/api.ts]
    end

    subgraph Backend [".NET 10 Web API"]
        Controllers[Controllers]
        Services[Services — lógica de negocio]
        Repositories[Repositories — acceso a datos]
        Middleware[Middleware — JWT / Errores]
    end

    subgraph Data ["Base de datos"]
        SQL[(SQL Server 2022)]
    end

    UI --> APIClient
    APIClient -->|HTTP + JWT| Controllers
    Controllers --> Services
    Services --> Repositories
    Repositories -->|EF Core| SQL
    Middleware -.->|intercepta| Controllers
```

## Descripción de capas

| Capa | Responsabilidad |
|:---|:---|
| **Controllers** | Recibe la request HTTP, valida el formato de entrada, delega al servicio |
| **Services** | Contiene la lógica de negocio. No conoce HTTP ni la BD directamente |
| **Repositories** | Único punto de acceso a la base de datos vía EF Core |
| **Middleware** | Validación de JWT en rutas protegidas. Manejo centralizado de errores |
| **DTOs** | Contratos de entrada/salida de la API — desacoplan el modelo de la respuesta HTTP |
