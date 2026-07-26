# TaskFlow

A full-stack task manager build using .NET 
Users register, log in, and perform CRUD on their own tasks (title, description,
status, due date). Built with **.NET 10 + Clean Architecture** (Dapper + SQLite)
and a **React + TypeScript** SPA.

> **Status:** feature-complete through **Iteration 4**. Iterations 0–3 delivered
> the Clean Architecture backend — `GET /api/ping`, the **Dapper repository
> layer**, the **task business logic** (`TaskService`), the **CRUD API**, and
> **JWT authentication** (register/login/me, BCrypt, `[Authorize]`-protected
> routes, per-user ownership). Iteration 4 adds the **React SPA**: login/register,
> a task board (create, edit, status change, delete) with the bearer token held in
> a React context. See [`docs/PLAN.md`](docs/PLAN.md); Iteration 5 covers final
> hardening.

## Documentation

| Doc | Purpose |
|-----|---------|
| [`docs/SPEC.md`](docs/SPEC.md) | User story, architecture, data model, API contract, testing strategy |
| [`docs/PLAN.md`](docs/PLAN.md) | Iteration-by-iteration TDD build plan |
| [`docs/PROMPTS.md`](docs/PROMPTS.md) | The prompt library driving the build **and the required GenAI write-up** |

## Architecture

Clean Architecture; dependencies point inward only.

```
TaskFlow.Api            → ASP.NET Core Web API + MVC, serves the React SPA
  └─ TaskFlow.Infrastructure  → Dapper repositories, SQLite, BCrypt, JWT   (repository layer)
       └─ TaskFlow.Application → services, validation, DTOs, abstractions   (business layer)
            └─ TaskFlow.Domain → entities, enums, repository interfaces     (no dependencies)
TaskFlow.UnitTests      → xUnit + Moq + FluentAssertions
client/                 → React + TypeScript (Vite)
```

The **business layer is independent of the data layer and the API**: it talks to
the database only through repository interfaces defined in the Domain.

## Tech stack

.NET 10 · ASP.NET Core · Dapper · SQLite · JWT + BCrypt · React 18 + TypeScript ·
Vite · xUnit / Moq / FluentAssertions.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org)

## Getting started

### 1. Backend (API on http://localhost:5080)

```bash
dotnet restore
dotnet run --project src/TaskFlow.Api
```

The database (`taskflow.db`) is created and seeded automatically on first run.

- Health check: <http://localhost:5080/api/ping>
- Swagger UI: <http://localhost:5080/swagger>

### 2. Frontend (Vite dev server on http://localhost:5173)

```bash
cd client
npm install
npm run dev
```

Open <http://localhost:5173>. The Vite dev server proxies `/api/*` to the .NET
host, so the browser uses a single origin. You should see **"API online"**.

### Production / single deployable

`npm run build` in `client/` emits the SPA into `src/TaskFlow.Api/wwwroot`; the
API then serves it via the SPA fallback endpoint (`MapFallbackToFile`), so
`dotnet run` alone serves both API and UI.

## Seeded demo credentials

| Email | Password |
|-------|----------|
| `demo@taskflow.dev` | `Passw0rd!` |

The demo user comes with a few sample tasks. To call the protected task
endpoints, log in first and send the returned token as `Authorization: Bearer <token>`:

```bash
# Get a token
curl -s -X POST http://localhost:5080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"demo@taskflow.dev","password":"Passw0rd!"}'

# Use it
curl -s http://localhost:5080/api/tasks -H "Authorization: Bearer <token>"
```

In Swagger, click **Authorize** and paste the token. Note: the `Jwt:SigningKey`
in `appsettings.json` is a development placeholder — replace it (e.g. via user
secrets or environment variables) for anything beyond local use.

## Tests

```bash
dotnet test
```

## Repository layout

```
TaskFlow.sln
src/
  TaskFlow.Domain/          entities, enums, repository interfaces
  TaskFlow.Application/     business logic, validation, DTOs, abstractions
  TaskFlow.Infrastructure/  Dapper repositories, SQLite, security, DI
  TaskFlow.Api/             controllers, Program.cs, SPA hosting
tests/
  TaskFlow.UnitTests/
client/                     React + TypeScript SPA
docs/                       SPEC, PLAN, PROMPTS
```

## GenAI

This project was built with GenAI assistance using an explicit, structured
prompting workflow. The prompts and a critical evaluation of AI-generated code
(validation, corrections, edge cases/auth/validation) are in
[`docs/PROMPTS.md`](docs/PROMPTS.md).
