using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace CodeSage.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<CodeChunkEntity> CodeChunks => Set<CodeChunkEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<CodeChunkEntity>(e =>
        {
            e.HasKey(x => x.Id);
            // matches nomic-embed-text dimensions.
            e.Property(x => x.Embedding).HasColumnType("vector(768)");
            e.HasIndex(x => x.FilePath);
        });
    }
}