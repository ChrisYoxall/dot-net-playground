namespace Web.Store.Backend.Domain;

/*
 
Entity Framework Core relies heavily on conventions.
 
A class needs a primary key. We’re using an EF Core naming convention that tells EF Core
that the property BookId is the primary key.

The AuthorId foreign key is used in the database to link a row in the Books table to a
row in the Author table.

The Author property is an EF Core navigational property. EF Core uses this on a save
to see whether the Book has an Author class attached. If so, it sets the foreign key, AuthorId.

Upon loading a Book class, the method Include will fill this property with the Author class
that’s linked to this Book class by using the foreign key, AuthorId.
 
 */

public class Book
{
    public int BookId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateTime PublishedOn { get; set; }
    public int AuthorId{ get; set; }
    public required Author Author{ get; set; }
}