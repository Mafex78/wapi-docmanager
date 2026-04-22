using Shared.Domain;
using WAPIIdentity.Domain.Entities;

namespace WAPIIdentity.Domain.Repositories;

public interface IUserRepository : IGenericRepository<User, string>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}