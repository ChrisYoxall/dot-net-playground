using EntityFrameworkDemo.Domain;
using EntityFrameworkDemo.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EntityFrameworkDemo.Tests;

public class UnitTests: IAsyncLifetime
{
    // Configure the PostgreSQL container
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18-alpine").Build();

    public async Task InitializeAsync()
    {
        // Start the container before tests run
        await _dbContainer.StartAsync();

        // Create the schema in the containerized database
        await using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        // Stop and destroy the container after tests finish
        await _dbContainer.DisposeAsync().AsTask();
    }

    // Helper method to generate fresh DbContext instances pointing to the container
    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task Can_Write_And_Read_Product_From_Database()
    {
        // Arrange: Insert a product using one DbContext instance
        var newProduct = new Product 
        { 
            Name = "Mechanical Keyboard", 
            Price = 150.00m 
        };

        await using (var writeContext = CreateDbContext())
        {
            writeContext.Products.Add(newProduct);
            await writeContext.SaveChangesAsync();
        }

        // Act: Read the product back using a BRAND NEW DbContext instance.
        // This ensures EF Core actually queries the database, rather than serving a cached/tracked entity
        Product? retrievedProduct;
        await using (var readContext = CreateDbContext())
        {
            retrievedProduct = await readContext.Products
                .FirstOrDefaultAsync(p => p.Name == "Mechanical Keyboard");
        }

        // Assert
        Assert.NotNull(retrievedProduct);
        Assert.Equal(newProduct.Id, retrievedProduct.Id);
        Assert.Equal("Mechanical Keyboard", retrievedProduct.Name);
        Assert.Equal(150.00m, retrievedProduct.Price);
    }
}