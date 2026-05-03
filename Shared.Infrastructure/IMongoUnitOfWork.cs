using MongoDB.Driver;
using Shared.Domain;

namespace Shared.Infrastructure;

public interface IMongoUnitOfWork : IUnitOfWork
{
    IClientSessionHandle? Session { get; }
}