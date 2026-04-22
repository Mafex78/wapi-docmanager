using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using WAPIDocument.Domain.Entities.Documents;

namespace WAPIDocument.Infrastructure;

public class DocumentsDbContext : DbContext
{
    public DocumentsDbContext(DbContextOptions<DocumentsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(b =>
        {
            b.ToCollection("documents");
            b.HasKey(x => x.Id);
            b.Property(x => x.Version).IsRowVersion();
            b.OwnsOne(x => x.Customer);
            b.OwnsMany(x => x.DocumentLines);
            b.OwnsMany(x => x.LinkedDocuments);
        });
    }
}