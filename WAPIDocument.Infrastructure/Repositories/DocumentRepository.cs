using Shared.Infrastructure;
using WAPIDocument.Domain.Entities.Documents;
using WAPIDocument.Domain.Repositories;

namespace WAPIDocument.Infrastructure.Repositories;

public class DocumentRepository : GenericRepository<Document, string>, IDocumentRepository
{
    public DocumentRepository(DocumentsDbContext dbContext) 
        : base(dbContext)
    {
    }
}