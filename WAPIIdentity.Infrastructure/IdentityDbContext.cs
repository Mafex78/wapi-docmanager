using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using WAPIIdentity.Domain.Entities;

namespace WAPIIdentity.Infrastructure;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.ToCollection("users");
            b.HasKey(x => x.Id);
            b.Property(x => x.Version).IsRowVersion();
        });
    }
}