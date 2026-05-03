using MongoDB.Driver;
using Shared.Domain;
using Shared.Infrastructure;
using WAPIIdentity.Domain.Entities;
using WAPIIdentity.Domain.Repositories;

namespace WAPIIdentity.Infrastructure.Repositories;

public class UserMongoRepository : MongoGenericRepository<User>, IUserRepository
{
    public UserMongoRepository(IMongoDatabase mongoDatabase, IUnitOfWork unitOfWork) 
        : base(mongoDatabase, unitOfWork, "users")
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;
            
        return await GetByFilterAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    }
}