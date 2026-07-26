using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.UnitTests.Infrastructure;
using Xunit;

namespace TaskFlow.UnitTests.Infrastructure;

public class TaskRepositoryTests : IDisposable
{
    private readonly SqliteTestDatabase _db = new();
    private readonly TaskRepository _sut;
    private readonly UserRepository _users;

    public TaskRepositoryTests()
    {
        _sut = new TaskRepository(_db.Factory);
        _users = new UserRepository(_db.Factory);
    }

    public void Dispose() => _db.Dispose();

    private async Task<Guid> CreateUserAsync(string email)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = email,
            PasswordHash = "hash",
            CreatedAtUtc = DateTime.UtcNow
        };
        await _users.AddAsync(user);
        return user.Id;
    }

    private static TaskItem NewTask(Guid userId, string title = "Task") => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Title = title,
        Description = "desc",
        Status = TaskState.Todo,
        DueDateUtc = DateTime.UtcNow.AddDays(1),
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow
    };

    [Fact]
    public async Task AddAsync_then_GetByIdForUserAsync_roundtrips_all_fields()
    {
        var userId = await CreateUserAsync("u1@example.com");
        var task = NewTask(userId);

        await _sut.AddAsync(task);
        var loaded = await _sut.GetByIdForUserAsync(task.Id, userId);

        loaded.Should().NotBeNull();
        loaded!.Title.Should().Be(task.Title);
        loaded.Description.Should().Be(task.Description);
        loaded.Status.Should().Be(TaskState.Todo);
        loaded.DueDateUtc.Should().BeCloseTo(task.DueDateUtc!.Value, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetAllForUserAsync_returns_only_the_owners_tasks()
    {
        var alice = await CreateUserAsync("alice@example.com");
        var bob = await CreateUserAsync("bob@example.com");
        await _sut.AddAsync(NewTask(alice, "Alice 1"));
        await _sut.AddAsync(NewTask(alice, "Alice 2"));
        await _sut.AddAsync(NewTask(bob, "Bob 1"));

        var aliceTasks = await _sut.GetAllForUserAsync(alice);

        aliceTasks.Should().HaveCount(2);
        aliceTasks.Select(t => t.Title).Should().BeEquivalentTo("Alice 1", "Alice 2");
    }

    [Fact]
    public async Task GetByIdForUserAsync_returns_null_for_another_users_task()
    {
        var owner = await CreateUserAsync("owner@example.com");
        var intruder = await CreateUserAsync("intruder@example.com");
        var task = NewTask(owner);
        await _sut.AddAsync(task);

        var loaded = await _sut.GetByIdForUserAsync(task.Id, intruder);

        loaded.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_persists_changes_and_returns_true()
    {
        var userId = await CreateUserAsync("u@example.com");
        var task = NewTask(userId);
        await _sut.AddAsync(task);

        task.Title = "Updated";
        task.Status = TaskState.Done;
        task.UpdatedAtUtc = DateTime.UtcNow;
        var updated = await _sut.UpdateAsync(task);

        updated.Should().BeTrue();
        var loaded = await _sut.GetByIdForUserAsync(task.Id, userId);
        loaded!.Title.Should().Be("Updated");
        loaded.Status.Should().Be(TaskState.Done);
    }

    [Fact]
    public async Task UpdateAsync_returns_false_for_another_users_task()
    {
        var owner = await CreateUserAsync("owner@example.com");
        var intruder = await CreateUserAsync("intruder@example.com");
        var task = NewTask(owner);
        await _sut.AddAsync(task);

        var stolen = NewTask(intruder);
        stolen.Id = task.Id; // same task id, wrong owner
        var updated = await _sut.UpdateAsync(stolen);

        updated.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_removes_the_task_and_returns_true()
    {
        var userId = await CreateUserAsync("u@example.com");
        var task = NewTask(userId);
        await _sut.AddAsync(task);

        var deleted = await _sut.DeleteAsync(task.Id, userId);

        deleted.Should().BeTrue();
        (await _sut.GetByIdForUserAsync(task.Id, userId)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_returns_false_for_another_users_task()
    {
        var owner = await CreateUserAsync("owner@example.com");
        var intruder = await CreateUserAsync("intruder@example.com");
        var task = NewTask(owner);
        await _sut.AddAsync(task);

        var deleted = await _sut.DeleteAsync(task.Id, intruder);

        deleted.Should().BeFalse();
    }
}
