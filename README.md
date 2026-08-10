# Task Tracker

Sistema de gestión de tareas de equipo — prueba técnica Desarrollador Fullstack.

## Stack

| Capa | Tecnología |
|:---|:---|
| Frontend | Next.js 14 (App Router) |
| Backend | .NET 10 Web API |
| Base de datos | SQL Server 2022 (Docker) |
| ORM | Entity Framework Core |
| Auth | JWT |

## Inicio rápido

### Prerrequisitos
- Docker Desktop
- .NET 10 SDK
- Node.js 20+

### Levantar la base de datos
```bash
cp .env.example .env
# Editar .env con tus valores
docker-compose up -d
```

### Backend
```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```

### Frontend
```bash
cd frontend
npm install
npm run dev
```

La app estará disponible en `http://localhost:3000`.
La API estará disponible en `http://localhost:5000`.

## Documentación

- [Arquitectura del sistema](docs/architecture.md)
- [Referencia de la API](docs/api.md)
- [Instalación detallada](docs/setup.md)
- [Flujo de trabajo con IA](AGENTIC_WORKFLOW.md)
