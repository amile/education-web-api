using EducationWebApi.Domain;

namespace EducationWebApi.Application;

public interface IUsersRepository
{
    Task<User?> GetUserByIdAsync(Guid login, CancellationToken ct = default);
    Task<User?> GetUserByLoginAsync(string login, CancellationToken ct = default);
    Task<int> GetUserActiveBookingsCountAsync(Guid userId, CancellationToken ct = default);
    Task AddUserAsync(User item, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
} 