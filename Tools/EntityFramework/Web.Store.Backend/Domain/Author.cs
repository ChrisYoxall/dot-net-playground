namespace Web.Store.Backend.Domain;

/*

The AuthorId property holds the primary key of the Author row in the DB. The foreign key in
the Book class has the same name.

 */

public class Author
{
    public int AuthorId { get; set; }
    public required string Name { get; set; }
}