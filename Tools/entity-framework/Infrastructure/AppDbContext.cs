using EntityFrameworkDemo.Domain;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkDemo.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Simple configuration demo
        modelBuilder.Entity<Product>()
            .Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        base.OnModelCreating(modelBuilder);
    }
}