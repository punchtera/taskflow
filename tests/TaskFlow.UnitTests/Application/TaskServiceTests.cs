using FluentAssertions;
using Moq;
using TaskFlow.Application.Common;
using TaskFlow.Application.Contracts;
using TaskFlow.Application.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Repositories;
using Xunit;

namespace TaskFlow.UnitTests.Application;

/// <summary>
/// Business-layer tests: the repository is mocked, so these assert rules,
/// mapping, ownership and timestamps without any database.
/// </summary>
public class TaskServiceTests
{
    private static readonly DateTime Now = new(2026, 07, 24, 12, 0, 0, DateTimeKind.Utc);
    private readonly Mock<ITaskRepository> _repo = new();
    private readonly TaskService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public TaskServiceTests()
    {
        var clock = new FakeTimeProvider(Now);
        _sut = new TaskService(_repo.Object, clock);
    }

    private TaskItem Existing(Guid id) => new()
    {
        Id = id,
        UserId = _userId,
        Title = "Old",
        Description = "old",
        Status = TaskState.Todo,
        DueDateUtc = Now.AddDays(1),
        CreatedAtUtc = Now.AddDays(-1),
        UpdatedAtUtc = Now.AddDays(-1)
    };

    [Fact]
    public async Task CreateAsync_persists_and_returns_mapped_response()
    {
        var req = new CreateTaskRequest("  Write report  ", "details", TaskState.InProgress, Now.AddDays(3));

        var result = await _sut.CreateAsync(_userId, req);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Write report"); // trimmed
        result.Value.Status.Should().Be(TaskState.InProgress);
        result.Value.CreatedAtUtc.Should().Be(Now);
        _repo.Verify(r => r.AddAsync(
            It.Is<TaskItem>(t => t.UserId == _userId && t.Title == "Write report" && t.CreatedAtUtc == Now),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_fails_validation_and_does_not_touch_repository()
    {
        var req = new CreateTaskRequest("", null, TaskState.Todo, null);

        var result = await _sut.CreateAsync(_userId, req);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Validation);
        _repo.Verify(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_returns_NotFound_when_absent()
    {
        _repo.Setup(r => r.GetByIdForUserAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync((TaskItem?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), _userId);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_returns_NotFound_when_task_missing()
    {
        _repo.Setup(r => r.GetByIdForUserAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync((TaskItem?)null);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), _userId,
            new UpdateTaskRequest("New", null, TaskState.Done, null));

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_applies_changes_and_bumps_UpdatedAt()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdForUserAsync(id, _userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Existing(id));
        _repo.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        var result = await _sut.UpdateAsync(id, _userId,
            new UpdateTaskRequest("New title", "new", TaskState.Done, Now.AddDays(2)));

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("New title");
        result.Value.Status.Should().Be(TaskState.Done);
        result.Value.UpdatedAtUtc.Should().Be(Now);
        _repo.Verify(r => r.UpdateAsync(
            It.Is<TaskItem>(t => t.Id == id && t.Title == "New title" && t.UpdatedAtUtc == Now),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_returns_NotFound_when_repository_deletes_nothing()
    {
        _repo.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), _userId);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_succeeds_when_repository_removes_the_row()
    {
        _repo.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), _userId);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_maps_every_task()
    {
        _repo.Setup(r => r.GetAllForUserAsync(_userId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new[] { Existing(Guid.NewGuid()), Existing(Guid.NewGuid()) });

        var result = await _sut.GetAllAsync(_userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }
}
