using Microsoft.EntityFrameworkCore;

namespace PasteQL.Data;

public class PasteDbContext : DbContext
{
    public PasteDbContext(DbContextOptions<PasteDbContext> options)
        : base(options)
    {
    }

    public DbSet<PasteRecord> Pastes => Set<PasteRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PasteRecord>()
            .HasIndex(x => x.Slug)
            .IsUnique();

        modelBuilder.Entity<PasteRecord>()
            .Property(x => x.Title)
            .HasMaxLength(200);

        modelBuilder.Entity<PasteRecord>()
            .Property(x => x.Language)
            .HasMaxLength(50);

        base.OnModelCreating(modelBuilder);
    }
}
