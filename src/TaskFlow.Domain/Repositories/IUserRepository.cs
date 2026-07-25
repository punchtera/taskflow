using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

/// <summary>
/// Persistence contract for <see cref="User"/>. Defined in the Domain so that
/// higher layers depend on the abstraction, not on the concrete data store.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}
