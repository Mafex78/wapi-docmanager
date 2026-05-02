using MongoDB.Driver;
using Shared.Infrastructure;
using WAPIDocument.Domain.Entities.Documents;
using WAPIDocument.Domain.Repositories;

namespace WAPIDocument.Infrastructure.Repositories;

public class DocumentMongoRepository : MongoGenericRepository<Document>, IDocumentRepository
{
    public DocumentMongoRepository(IMongoDatabase mongoDatabase) 
        : base(mongoDatabase, "documents")
    {
    }
}