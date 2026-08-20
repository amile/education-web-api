using System.Data;
using EducationWebApi.Application;
using EducationWebApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace EducationWebApi.Infrastructure;

public class UsersRepository : IUsersRepository
{
    private readonly AppDbContext _dbContext;

    public UsersRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(e => e.Id == userId, ct);

        return user is not null ? UserFactory.FromDb(user) : null;
    }

    public async Task<User?> GetUserByLoginAsync(string login, CancellationToken ct = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(e => e.Login == login, ct);

        return user is not null ? UserFactory.FromDb(user) : null;
    }

    public Task AddUserAsync(User item, CancellationToken ct = default)
    {
        return _dbContext.AddAsync(UserFactory.ToDb(item), ct).AsTask();
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}
