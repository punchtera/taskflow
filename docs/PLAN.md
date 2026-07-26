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

## Iteration 2 — Task business logic + CRUD API ✅

**Goal:** end-to-end task CRUD through the business layer.

- ✅ *(test first)* `TaskServiceTests` with a mocked `ITaskRepository` covering rules, not-found, mapping, timestamps and trimming (fixed-clock `FakeTimeProvider`).
- ✅ Implement `TaskService` (maps DTOs ↔ entities, applies `TaskRules`, sets server-owned timestamps via injected `TimeProvider`, returns typed `Result`).
- ✅ `TasksController` with GET/GET{id}/POST/PUT/DELETE; thin controller mapping `Result.Error` → HTTP codes (400/404/409/401) with ProblemDetails; 201+Location on create, 204 on delete.
- ✅ `ICurrentUserAccessor` abstraction; interim `DemoCurrentUserAccessor` targets the seeded demo user until auth (Iteration 3).
- ✅ *(test)* API-level tests via `WebApplicationFactory` against a throwaway SQLite DB (list, create+roundtrip, 400, 404, update, delete).
- ✅ `AddApplication()` DI entry point; registered `TimeProvider.System`.

**Acceptance:** all task endpoints behave per the API contract with correct codes.

---

## Iteration 3 — Auth (second API) ✅

**Goal:** registration, login, and protected endpoints.

- ✅ *(test first)* `AuthServiceTests` — duplicate email → Conflict, bad/unknown password → Unauthorized, bad input → Validation, happy path issues a token.
- ✅ `JwtTokenService` in Infrastructure implementing `ITokenService`; `JwtSettings` bound from the `Jwt` section.
- ✅ Implement `AuthService` (register hashes password + issues JWT; login verifies and issues JWT; generic failure message avoids field-level leaks). `AuthRules` validation.
- ✅ `AuthController`: `register` (201), `login` (200), `me` (authorized); `register`/`login`/`ping` are the non-authorized endpoints.
- ✅ JWT bearer authentication (`MapInboundClaims=false`, raw `sub`); `[Authorize]` on `TasksController`; `HttpCurrentUserAccessor` resolves the caller's `UserId` from claims, replacing the interim demo accessor.
- ✅ *(test)* API tests: 401 without token, register/login/me, and cross-user isolation (a new user sees none of the demo user's tasks).

**Acceptance:** protected routes return 401 without a token and work with one; ownership enforced.

---

## Iteration 4 — React CRUD UI ✅

**Goal:** the full use case in the browser.

- ✅ `AuthContext`/`useAuth` holding token + user in memory; bearer token injected centrally in `api/client.ts` (`setAuthToken`).
- ✅ `LoginForm` with login/register toggle and inline validation/error handling.
- ✅ Task board grouped into To do / In progress / Done columns with create, inline edit, quick status change and delete via the `useTasks` hook; loading and error (with retry) states.
- ✅ Responsive styling (columns stack on small screens); accessible labels/roles; no `localStorage` and no console warnings.
- ✅ Clean structure: `api/`, `components/`, `context/`, `hooks/`, `utils/`.

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
