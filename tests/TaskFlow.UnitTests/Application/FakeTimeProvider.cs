namespace TaskFlow.UnitTests.Application;

/// <summary>
/// Minimal fixed-clock <see cref="TimeProvider"/> so time-dependent business
/// logic is deterministic in tests (avoids taking a dependency on the
/// Microsoft.Extensions.TimeProvider.Testing package).
/// </summary>
internal sealed class FakeTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public FakeTimeProvider(DateTime utcNow) => _utcNow = new DateTimeOffset(utcNow, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _utcNow;
}
