using Microsoft.EntityFrameworkCore;
using PersonApi.Domain.Entities;

namespace PersonApi.Infrastructure.Data;

/// <summary>
/// Application database context for Entity Framework Core
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Person> Persons { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Person entity
        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Age)
                .IsRequired();

            entity.Property(e => e.DateOfBirth)
                .IsRequired();

            entity.Property(e => e.Skills)
                .HasMaxLength(2000)
                .HasDefaultValue("[]");

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt);

            // Indexes
            entity.HasIndex(e => e.Name);
        });
    }
}
