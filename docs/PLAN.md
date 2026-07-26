# TaskFlow — Implementation Plan

An incremental, TDD-first plan. Each iteration is a vertical slice that leaves
the app building and green. Work happens on the `dev` branch; each iteration can
be a short-lived feature branch merged into `dev`.

Legend: ✅ done in this starter commit · ⬜ to be built in a following iteration.

---

## Iteration 0 — Foundation ✅

**Goal:** a runnable skeleton that proves the architecture and the full-stack wiring.

- ✅ Clean Architecture solution: Domain, Application, Infrastructure, Api, Tests.
- ✅ Domain entities (`User`, `TaskItem`), `TaskState` enum, repository interfaces.
- ✅ Application scaffolding: `Result`/`Error`, DTOs, service interfaces, `TaskRules` validation.
- ✅ Infrastructure: SQLite connection factory, `DbInitializer` (schema + seed), BCrypt hasher, DI entry point.
- ✅ API: `GET /api/ping`, Swagger, CORS for the SPA, DB init on startup, SPA fallback endpoint.
- ✅ React + TypeScript (Vite) client that calls `/api/ping` and reports API health.
- ✅ First unit tests (`TaskRulesTests`) establishing the TDD loop.

**Acceptance:** `dotnet build` succeeds; `GET /api/ping` → 200; `npm run dev` shows "API online".

---

## Iteration 1 — Data-access (repository) layer ✅

**Goal:** persist and retrieve entities through Dapper.

- ✅ *(test first)* `UserRepositoryTests` and `TaskRepositoryTests` against a temp SQLite DB (`SqliteTestDatabase`); schema extracted to `DatabaseSchema` for clean, seed-free test databases.
- ✅ Implement `UserRepository` (`GetById`, `GetByEmail` (case-insensitive), `ExistsByEmail`, `Add`).
- ✅ Implement `TaskRepository` (`GetAllForUser`, `GetByIdForUser`, `Add`, `Update`, `Delete`) — every query scoped by `UserId`.
- ✅ Explicit TEXT↔Guid/DateTime mapping via `SqliteValueConverter`; parameterised SQL throughout.
- ✅ Register both as scoped services in `Infrastructure.DependencyInjection`.

**Acceptance:** repository tests green; CRUD round-trips and user-scoping verified against SQLite.

---

## Iteration 2 — Task business logic + CRUD API ⬜

**Goal:** end-to-end task CRUD through the business layer.

- ⬜ *(test first)* `TaskServiceTests` with a mocked `ITaskRepository` covering rules, not-found and ownership.
- ⬜ Implement `TaskService` (maps DTOs ↔ entities, applies `TaskRules`, sets timestamps, returns typed `Result`).
- ⬜ `TasksController` with GET/GET{id}/POST/PUT/DELETE mapping `Result` → HTTP status codes.
- ⬜ *(test)* API-level tests via `WebApplicationFactory`.

**Acceptance:** all task endpoints behave per the API contract with correct codes.

---

## Iteration 3 — Auth (second API) ⬜

**Goal:** registration, login, and protected endpoints.

- ⬜ *(test first)* `AuthServiceTests` — duplicate email → Conflict, bad password → Unauthorized, happy path issues a token.
- ⬜ `JwtTokenService` in Infrastructure implementing `ITokenService`; bind `Jwt` settings.
- ⬜ Implement `AuthService` (register hashes password; login verifies and issues JWT).
- ⬜ `AuthController`: `register`, `login`, `me` (authorized), and confirm a non-authorized endpoint exists.
- ⬜ Add JWT bearer authentication + `[Authorize]`; resolve the caller's `UserId` from claims and enforce it on all task routes.

**Acceptance:** protected routes return 401 without a token and work with one; ownership enforced.

---

## Iteration 4 — React CRUD UI ⬜

**Goal:** the full use case in the browser.

- ⬜ Auth context + login/register screens; attach bearer token in the API client.
- ⬜ Task list/board with create, edit, status change, delete; loading & error states.
- ⬜ Responsive styling; verify **no console warnings**.
- ⬜ Clean component/state structure (`components/`, `hooks/`, `context/`, `api/`).

**Acceptance:** a user can register/log in and manage tasks end-to-end.

---

## Iteration 5 — Hardening & submission ⬜

- ⬜ Global exception-handling middleware → ProblemDetails.
- ⬜ Fill test coverage gaps across all three layers; confirm TDD story holds.
- ⬜ Finalize README (setup, run, seeded credentials, screenshots) and the GenAI write-up.
- ⬜ Final pass: no build warnings, no console warnings, tidy commit history on `dev`.

---

## Definition of done (per iteration)

1. Tests written first and passing.
2. `dotnet build` and `dotnet test` green; no new warnings.
3. Layer boundaries respected (no inward→outward references).
4. Committed to `dev` with a clear message.
