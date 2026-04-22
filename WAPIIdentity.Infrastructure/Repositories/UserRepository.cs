using Shared.Infrastructure;
using WAPIIdentity.Domain.Entities;
using WAPIIdentity.Domain.Repositories;

namespace WAPIIdentity.Infrastructure.Repositories;

public class UserRepository: GenericRepository<User, string>, IUserRepository
{
    public UserRepository(IdentityDbContext dbContext) 
        : base(dbContext)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;
            
        return await GetByFilterAsync(u => u.Email!.ToLower() == email.ToLower(), cancellationToken);
    }
}