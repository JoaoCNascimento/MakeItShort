using MakeItShort.API.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace MakeItShort.API.Repository;

public class MakeItShortDbContext : DbContext
{
    public DbSet<ShortUrl> ShortUrls { get; set; }

    public MakeItShortDbContext() { }

    public MakeItShortDbContext(DbContextOptions<MakeItShortDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortUrl>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<ShortUrl>()
            .HasIndex(e => e.ShortKey)
            .IsUnique();
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker
            .Entries<ShortUrl>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}