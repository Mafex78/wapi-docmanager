using Shared.Domain;
using WAPIDocument.Domain.Entities.Documents;

namespace WAPIDocument.Domain.Repositories;

public interface IDocumentRepository : IGenericRepository<Document, string>
{
}