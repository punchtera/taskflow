# TaskFlow — Prompts & GenAI Write-up

This file documents how GenAI tooling was used to build TaskFlow. Every prompt
follows a three-part framework:

1. **Setting the stage** — context and objective.
2. **Defining the task** — the specific action, with relevant details.
3. **Specifying the rules** — style, format, constraints and quality bars.

Part A is the reusable prompt library that drives the iterations in `PLAN.md`.
Part B is the mandatory GenAI section of the exercise.

---

## Part A — Iterative build prompts

### A0 · Master / system prompt (set once per session)

> **Setting the stage:** You are pairing with me on a .NET 9 full-stack coding
> exercise called *TaskFlow* — a task manager where users register, log in and
> perform CRUD on their own tasks (title, description, status, due date). The
> solution follows **Clean Architecture** with four projects — `Domain`,
> `Application` (business layer), `Infrastructure` (repository layer, Dapper +
> SQLite), and `Api` (ASP.NET Core) — plus an xUnit test project and a React +
> TypeScript (Vite) client. The full contract is in `docs/SPEC.md` and the
> roadmap is in `docs/PLAN.md`.
>
> **Defining the task:** Help me implement the plan one iteration at a time. For
> each iteration, write the tests first, then the minimum code to pass them, then
> refactor. Touch only the files the current iteration needs.
>
> **Specifying the rules:**
> - Respect layer boundaries: dependencies point inward only; the business layer
>   never references Infrastructure or ASP.NET types.
> - Data access goes through the Domain repository interfaces; SQL lives in
>   Infrastructure only.
> - Use the `Result`/`Error` type for expected failures; throw only for truly
>   exceptional cases.
> - Every task query is scoped by `UserId`; a user can never touch another user's data.
> - Idiomatic, nullable-aware C#; xUnit + Moq + FluentAssertions; async with `CancellationToken`.
> - After each change, tell me exactly which commands to run to verify, and stop
>   for my review before starting the next iteration.

### A1 · Repository layer (Iteration 1)

> **Setting the stage:** Iteration 0 (skeleton) is done. We now build the
> data-access layer described in `docs/PLAN.md` § Iteration 1.
>
> **Defining the task:** Implement `UserRepository` and `TaskRepository` using
> Dapper against SQLite, fulfilling the Domain interfaces `IUserRepository` and
> `ITaskRepository`. Write the tests first, running them against a temporary
> SQLite database created from `DbInitializer`.
>
> **Specifying the rules:** Map `Guid`/`DateTime`/enum to the SQLite TEXT/INTEGER
> columns exactly as in `SPEC.md` § 5. Every `TaskRepository` method filters by
> `UserId`. Parameterise all SQL. Register both repositories in
> `Infrastructure.DependencyInjection`. Do not change the API or business layers yet.

### A2 · Task business logic + CRUD API (Iteration 2)

> **Setting the stage:** Repositories exist and are tested. Build the task use
> case end to end per `docs/PLAN.md` § Iteration 2.
>
> **Defining the task:** Write `TaskServiceTests` against a mocked
> `ITaskRepository`, then implement `TaskService` (DTO↔entity mapping, apply
> `TaskRules`, set `CreatedAtUtc`/`UpdatedAtUtc`, return typed `Result`). Then add
> `TasksController` with GET, GET/{id}, POST, PUT, DELETE.
>
> **Specifying the rules:** Map `Result.Error.Type` to HTTP codes
> (Validation→400, NotFound→404, Conflict→409, Unauthorized→401); POST returns
> 201 + Location; DELETE returns 204. Add `WebApplicationFactory` tests for the
> happy path and a 404. Keep the controller thin — no business logic in it.

### A3 · Auth: the second API (Iteration 3)

> **Setting the stage:** Task CRUD works for a hard-coded user. Add real auth per
> `docs/PLAN.md` § Iteration 3.
>
> **Defining the task:** Implement `JwtTokenService` (Infrastructure) and
> `AuthService` with register and login, tests first. Add `AuthController`
> (`register`, `login`, `me`), enable JWT bearer authentication, protect the task
> routes with `[Authorize]`, and read the caller's `UserId` from the token claims.
>
> **Specifying the rules:** Hash passwords with the existing `IPasswordHasher`
> (BCrypt); never return the hash. Duplicate email → Conflict, bad credentials →
> Unauthorized (do not reveal which field failed). Bind JWT settings from
> `appsettings.json`. Replace any hard-coded user id with the authenticated one
> and confirm cross-user access returns 404, not 403 (don't leak existence).

### A4 · React CRUD UI (Iteration 4)

> **Setting the stage:** The API is complete and secured. Build the frontend use
> case per `docs/PLAN.md` § Iteration 4.
>
> **Defining the task:** Add an auth context with login/register screens and a
> task board supporting create, edit, status change and delete, using the
> existing `api/client.ts` (extend it to attach the bearer token).
>
> **Specifying the rules:** TypeScript, function components and hooks; organise
> into `api/`, `components/`, `hooks/`, `context/`. Responsive and accessible;
> handle loading and error states; **zero console warnings**. Keep all network
> calls in the api client, not in components.

### A5 · Debugging prompt (use as needed)

> **Setting the stage:** [paste the failing test / stack trace / console error].
> **Defining the task:** Diagnose the root cause and propose the smallest fix
> consistent with our architecture.
> **Specifying the rules:** Explain the cause before the fix; don't weaken a test
> to make it pass; don't cross layer boundaries to work around the problem.

---

## Part B — GenAI section (required by the exercise)

> *Exercise task: generate a RESTful API for a simple task-management system —
> CRUD on tasks (title, description, status, due_date) associated with a user.*

### B.1 The prompt I would use

> **Setting the stage:** You are a senior .NET engineer. Generate the scaffold for
> a RESTful **task-management API** in **C# / ASP.NET Core (.NET 9)** following
> **Clean Architecture** (Domain → Application → Infrastructure → Api). Persistence
> is **SQLite via Dapper**. A basic `User` model already exists; tasks belong to a user.
>
> **Defining the task:** Produce:
> 1. A `TaskItem` entity with `Id` (Guid), `UserId` (Guid), `Title`,
>    `Description`, `Status` (enum: Todo/InProgress/Done) and `DueDateUtc`.
> 2. An `ITaskRepository` interface (Domain) and a Dapper implementation
>    (Infrastructure), every query scoped by `UserId`.
> 3. A `TaskService` (Application) holding validation and mapping, returning a
>    `Result<T>` rather than throwing for expected failures.
> 4. A `TasksController` exposing GET, GET/{id}, POST, PUT, DELETE.
> 5. xUnit tests for the service (mocked repo) and the controller.
>
> **Specifying the rules:** Use correct REST verbs and status codes (201+Location
> on create, 204 on delete, 400/404 appropriately). Validate input (title
> required, ≤200 chars; due date not in the past). Require authentication and
> derive `UserId` from the caller's claims — never trust a client-supplied user
> id. Async with `CancellationToken`. Parameterised SQL only. No business logic in
> the controller. Return the code as separate files with their paths.

### B.2 Representative output (sample)

A representative slice of what the model produces from the prompt above — the
controller and the service. (The full generation includes the entity, repository
and tests; this repo's `src/` contains the reviewed, corrected versions.)

```csharp
// Api/Controllers/TasksController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _tasks;
    public TasksController(ITaskService tasks) => _tasks = tasks;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok((await _tasks.GetAllAsync(CurrentUserId, ct)).Value);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _tasks.GetByIdAsync(id, CurrentUserId, ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest req, CancellationToken ct)
    {
        var result = await _tasks.CreateAsync(CurrentUserId, req, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskRequest req, CancellationToken ct)
    {
        var result = await _tasks.UpdateAsync(id, CurrentUserId, req, ct);
        if (result.IsSuccess) return Ok(result.Value);
        return result.Error!.Type == ErrorType.NotFound ? NotFound() : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _tasks.DeleteAsync(id, CurrentUserId, ct);
        return result.IsSuccess ? NoContent() : NotFound();
    }
}
```

```csharp
// Application/Services/TaskService.cs (core of create/update)
public async Task<Result<TaskResponse>> CreateAsync(
    Guid userId, CreateTaskRequest req, CancellationToken ct = default)
{
    var validation = TaskRules.Validate(req, DateTime.UtcNow);
    if (validation.IsFailure) return Result.Failure<TaskResponse>(validation.Error!);

    var now = DateTime.UtcNow;
    var entity = new TaskItem
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Title = req.Title.Trim(),
        Description = req.Description,
        Status = req.Status,
        DueDateUtc = req.DueDateUtc,
        CreatedAtUtc = now,
        UpdatedAtUtc = now
    };

    await _repository.AddAsync(entity, ct);
    return Result.Success(Map(entity));
}
```

### B.3 How I validated the AI's suggestions

- **Compiled and ran the tests.** First gate: does it build, and are the generated
  tests actually meaningful (not asserting trivialities)?
- **Checked the architecture.** Confirmed the controller has no business logic,
  the service has no SQL, and the business layer references no ASP.NET or Dapper
  types — the model is prone to "helpfully" collapsing layers.
- **Read the SQL.** Verified queries are parameterised (no string concatenation)
  and that **every** task query filters by `UserId`.
- **Verified status-code semantics** against REST conventions (201+Location,
  204, 400 vs 404 vs 409).

### B.4 What I corrected or improved

- **Trust boundary:** the first output accepted `userId` from the request body /
  route. I changed it to derive `UserId` from the JWT claims so a client can't
  act as another user.
- **Error handling:** the model threw exceptions for "not found" and "validation".
  I replaced these with the `Result`/`Error` type so expected failures don't cost
  an exception and map cleanly to HTTP codes.
- **Info leak:** cross-user access originally returned 403 (revealing the record
  exists). Changed to 404.
- **Enum persistence:** it stored `Status` as text inconsistently; standardised on
  an INTEGER column matching the `TaskState` enum.
- **Timestamps:** added server-set `CreatedAtUtc`/`UpdatedAtUtc` the model omitted.

### B.5 Edge cases, authentication and validation

- **Validation:** title required and ≤200 chars, description ≤2000 chars, due date
  not in the past — centralised in `TaskRules` and unit-tested (`TaskRulesTests`).
- **Authentication:** JWT bearer; passwords stored only as BCrypt hashes; login
  failures are generic (no field-level leak).
- **Authorization / ownership:** every task operation is scoped to the caller's
  `UserId`; another user's id yields 404.
- **Edge cases handled:** empty/whitespace title, over-length fields, non-existent
  or foreign task id, missing/expired token, duplicate registration email,
  malformed GUID route (constrained by `{id:guid}`).

### B.6 Critical-thinking takeaway

GenAI is excellent for scaffolding boilerplate quickly, but it optimises for
"looks correct and compiles," not for security or architectural discipline. The
recurring risks — trusting client-supplied identity, collapsing layers, leaking
information through status codes, and shallow tests — are exactly what a human
review must catch. The AI accelerates the typing; the engineer owns the
correctness.
