using Microsoft.EntityFrameworkCore;
using Web.Store.Backend.Domain;

namespace Web.Store.Backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
}