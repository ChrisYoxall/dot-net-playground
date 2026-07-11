using System.Net;
using System.Net.Http.Json;
using Web.Store.Backend.Domain;

namespace Integration.Tests;

public class BookTests(StoreFixture store): IClassFixture<StoreFixture>
{
    [Fact]
    public async Task Can_Create_And_Retrieve_Book()
    {
        // Arrange
        var client = store.CreateClient();
        var newBook = new Book
        {
            Title = "Writing Integration Tests in .NET",
            Description = "An introductory guide to integration testing.",
            PublishedOn = DateTime.UtcNow,
            Author = new Author { Name = "Jane Doe" }
        };

        // Act - Post the book
        var postResponse = await client.PostAsJsonAsync("api/book", newBook, CancellationToken.None);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdBook = await postResponse.Content.ReadFromJsonAsync<Book>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdBook);
        Assert.True(createdBook.BookId > 0);
        Assert.Equal(newBook.Title, createdBook.Title);
        Assert.NotNull(createdBook.Author);
        Assert.Equal("Jane Doe", createdBook.Author.Name);

        // Act - Get the book list
        var getResponse = await client.GetAsync("api/book", CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var books = await getResponse.Content.ReadFromJsonAsync<List<Book>>(cancellationToken: TestContext.Current.CancellationToken);
        
        // Assert
        Assert.NotNull(books);
        var foundBook = books.SingleOrDefault(b => b.BookId == createdBook.BookId);
        Assert.NotNull(foundBook);
        Assert.Equal(createdBook.Title, foundBook.Title);
        Assert.Equal(createdBook.Author.Name, foundBook.Author.Name);
    }

    [Fact]
    public async Task Delete_NonExistent_Book_Returns_NotFound()
    {
        // Arrange
        var client = store.CreateClient();
        const int nonExistentBookId = 9999;

        // Act
        var deleteResponse = await client.DeleteAsync($"api/book/{nonExistentBookId}", CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }
}