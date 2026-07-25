# TaskFlow — Specification

> Technical specification for the Ballast Lane .NET full-stack exercise.
> This document is written *before* the code and is the single source of truth
> that the plan (`PLAN.md`) and prompts (`PROMPTS.md`) are derived from.

## 1. User story

> **As** a busy professional,
> **I want** to sign up, log in, and manage a private list of tasks (create, view,
> edit, complete, and delete them, each with a title, description, status and due
> date),
> **so that** I can keep track of my work in one place and only I can see my tasks.

Supporting stories:

- As a visitor, I can register with an email, display name and password.
- As a registered user, I can log in and receive a token that authorizes my requests.
- As a logged-in user, I can only ever read or modify **my own** tasks.
- As a reviewer, I can clone the repo, run it, and log in with seeded demo
  credentials without any manual data setup.

The domain (task management with `title`, `description`, `status`, `due_date`,
owned by a user) is deliberately the **same** domain used by the mandatory GenAI
section of the exercise, so the prompt work and the delivered app reinforce each
other.

## 2. Scope

### In scope

- Two resources: **Users** and **Tasks** (satisfies "at least two tables, each
  with a primary key and 2+ fields").
- CRUD Web API for tasks with correct HTTP verbs, status codes and validation.
- A second API surface for auth: register, login, plus an authorized and a
  non-authorized endpoint.
- JWT bearer authentication; passwords stored only as BCrypt hashes.
- A responsive React SPA implementing the task CRUD use case.
- Unit tests across the business layer, data-access layer and API layer (TDD).
- Seeded demo user and tasks.

### Out of scope (explicitly, to keep the exercise focused)

- Task sharing / multi-user collaboration.
- Refresh tokens, password reset, email verification.
- Pagination and full-text search (noted as possible extensions).

## 3. Architecture — Clean Architecture

Dependencies point **inward only**. Inner layers know nothing about outer ones.

```
┌─────────────────────────────────────────────────────────────┐
│  TaskFlow.Api  (ASP.NET Core Web API + MVC + React host)     │  ← outermost
│  Controllers, middleware, DI composition root, SPA serving   │
└───────────────▲──────────────────────────▲──────────────────┘
                │                           │
┌───────────────┴───────────┐   ┌───────────┴──────────────────┐
│  TaskFlow.Infrastructure   │   │  (references Application)     │
│  Dapper repositories,      │   │                              │
│  SQLite, BCrypt, JWT       │   │                              │
└───────────────▲────────────┘  └──────────────────────────────┘
                │
┌───────────────┴─────────────────────────────────────────────┐
│  TaskFlow.Application  (business logic layer)                 │
│  Services, DTOs/contracts, validation rules, abstractions     │
└───────────────▲──────────────────────────────────────────────┘
                │
┌───────────────┴──────────────────────────────────────────────┐
│  TaskFlow.Domain  (entities, enums, repository interfaces)     │  ← innermost
│  No dependencies on any other project or framework            │
└───────────────────────────────────────────────────────────────┘
```

| Layer | Project | Responsibility | Depends on |
|-------|---------|----------------|------------|
| Domain | `TaskFlow.Domain` | Entities (`User`, `TaskItem`), `TaskState` enum, repository **interfaces** | nothing |
| Application (**business layer**) | `TaskFlow.Application` | Services, business rules & validation, DTOs, hashing/token **abstractions**, `Result` type | Domain |
| Infrastructure (**repository layer**) | `TaskFlow.Infrastructure` | Dapper repository **implementations**, SQLite connection factory, DB initializer/seed, BCrypt hasher, JWT token service | Application, Domain |
| Presentation | `TaskFlow.Api` | Controllers, auth middleware, DI wiring, Swagger, serving the React SPA | Application, Infrastructure |
| Tests | `TaskFlow.UnitTests` | Unit tests per layer | Application, Domain (+ mocks) |

The **business layer is independent of the data layer and the API** (rubric
requirement): it references only the Domain and depends on data access through
the `ITaskRepository` / `IUserRepository` interfaces defined in the Domain.

## 4. Technology decisions

| Concern | Choice | Rationale |
|---------|--------|-----------|
| Runtime | **.NET 9** | Requested; latest SDK. |
| Data store | **SQLite** | Zero-install, single-file DB — a reviewer can clone and run. |
| Data access | **Dapper** (micro-ORM) | Lighter and more explicit than EF Core; makes the hand-written repository layer and SQL visible, which suits a code review. |
| Auth | **JWT bearer** + **BCrypt** | Standard stateless auth; BCrypt for salted password hashing. |
| Frontend | **React + TypeScript (Vite)** | Requested library; Vite dev server proxies `/api` to the .NET host. |
| Tests | **xUnit + Moq + FluentAssertions** | Idiomatic .NET testing stack. |

## 5. Data model

### Users

| Column | Type | Notes |
|--------|------|-------|
| Id | TEXT (GUID) | Primary key |
| Email | TEXT | Unique, case-insensitive |
| DisplayName | TEXT | |
| PasswordHash | TEXT | BCrypt hash — never plaintext |
| CreatedAtUtc | TEXT (ISO-8601) | |

### Tasks

| Column | Type | Notes |
|--------|------|-------|
| Id | TEXT (GUID) | Primary key |
| UserId | TEXT (GUID) | FK → Users(Id), `ON DELETE CASCADE`, indexed |
| Title | TEXT | Required, ≤ 200 chars |
| Description | TEXT | Optional, ≤ 2000 chars |
| Status | INTEGER | `TaskState`: 0 Todo, 1 InProgress, 2 Done |
| DueDateUtc | TEXT (ISO-8601) | Optional; not in the past on write |
| CreatedAtUtc | TEXT (ISO-8601) | Set by server |
| UpdatedAtUtc | TEXT (ISO-8601) | Set by server on every write |

## 6. API contract

Base path `/api`. All task endpoints require a valid bearer token and operate
only on the caller's own tasks.

| Method | Route | Auth | Body | Success | Errors |
|--------|-------|------|------|---------|--------|
| GET | `/api/ping` | no | — | 200 status payload | — |
| POST | `/api/auth/register` | no | `RegisterRequest` | 201 `AuthResponse` | 400, 409 email taken |
| POST | `/api/auth/login` | no | `LoginRequest` | 200 `AuthResponse` | 400, 401 |
| GET | `/api/auth/me` | **yes** | — | 200 current user | 401 |
| GET | `/api/tasks` | **yes** | — | 200 `TaskResponse[]` | 401 |
| GET | `/api/tasks/{id}` | **yes** | — | 200 `TaskResponse` | 401, 404 |
| POST | `/api/tasks` | **yes** | `CreateTaskRequest` | 201 `TaskResponse` | 400, 401 |
| PUT | `/api/tasks/{id}` | **yes** | `UpdateTaskRequest` | 200 `TaskResponse` | 400, 401, 404 |
| DELETE | `/api/tasks/{id}` | **yes** | — | 204 | 401, 404 |

`GET /api/ping` and `POST /api/auth/*` are the **non-authorized** endpoints;
everything under `/api/tasks` and `/api/auth/me` are the **authorized** ones.

### Error shape

Business failures map from the Application `Result`/`Error` type to HTTP:
`Validation → 400`, `Unauthorized → 401`, `NotFound → 404`, `Conflict → 409`,
each returning an RFC 7807-style problem payload.

## 7. Frontend

- Login / register screen; token held in memory (React state/context).
- Task board/list with create, edit (inline or modal), status toggle, delete.
- Responsive layout; no console warnings.
- Structured code: `api/` client, `components/`, `hooks/`, `context/`.

## 8. Testing strategy (TDD)

- **Business layer**: validation rules and service orchestration tested with
  mocked repositories (no DB).
- **Data layer**: repositories tested against a real SQLite in-memory/temp
  database to prove the SQL round-trips.
- **API layer**: controller/endpoint behaviour and status-code mapping via
  `WebApplicationFactory`.
- Red → green → refactor; the current `TaskRulesTests` demonstrate the pattern.

## 9. Acceptance criteria

- `dotnet test` is green.
- `dotnet run` starts the API; `GET /api/ping` returns 200.
- The SPA lists, creates, edits and deletes tasks against the running API.
- Seeded credentials log in successfully.
- Architecture, tests and GenAI write-up satisfy the exercise rubric.
