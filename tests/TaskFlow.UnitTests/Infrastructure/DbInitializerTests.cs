using Dapper;
using FluentAssertions;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Security;
using TaskFlow.UnitTests.Infrastructure;
using Xunit;

namespace TaskFlow.UnitTests.Infrastructure;

public class DbInitializerTests : IDisposable
{
    private readonly SqliteTestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public void Initialize_seeds_the_demo_user_and_tasks()
    {
        var sut = new DbInitializer(_db.Factory, new BCryptPasswordHasher());

        sut.Initialize();

        using var db = _db.Factory.Create();
        db.ExecuteScalar<long>("SELECT COUNT(1) FROM Users").Should().Be(1);
        db.ExecuteScalar<long>("SELECT COUNT(1) FROM Tasks").Should().BeGreaterThan(0);
    }

    [Fact]
    public void Initialize_is_idempotent_and_does_not_duplicate_seed_data()
    {
        var sut = new DbInitializer(_db.Factory, new BCryptPasswordHasher());

        sut.Initialize();
        using var db = _db.Factory.Create();
        var tasksAfterFirst = db.ExecuteScalar<long>("SELECT COUNT(1) FROM Tasks");

        sut.Initialize(); // run again

        db.ExecuteScalar<long>("SELECT COUNT(1) FROM Users").Should().Be(1);
        db.ExecuteScalar<long>("SELECT COUNT(1) FROM Tasks").Should().Be(tasksAfterFirst);
    }
}
