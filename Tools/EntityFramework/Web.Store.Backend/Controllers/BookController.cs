using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Store.Backend.Data;
using Web.Store.Backend.Domain;

namespace Web.Store.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController(AppDbContext context) : ControllerBase
    {
        // GET: api/Books
        // Reads all books. We use .Include() to eagerly load the Author data.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            return await context.Books
                .Include(b => b.Author)
                .ToListAsync();
            
            /*
             Could add .AsNoTracking() to the statement above 
             */
            
            
        }

        // POST: api/Books
        // Writes a new book to the database.
        [HttpPost]
        public async Task<ActionResult<Book>> PostBook(Book book)
        {
            /* This will create new (duplicated) books and authors if the same
             request is resent. */
            
            context.Books.Add(book);
            await context.SaveChangesAsync();

            // Returns a 201 Created response
            return CreatedAtAction(nameof(GetBooks), new { id = book.BookId }, book);
        }
        
        // DELETE: api/Books/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            /* Because we delete a child record (Book), Entity Framework Core will safely remove this specific book
             from the database but the parent record (Author) will remain completely untouched. The cascade delete
             created by EF only triggers in the opposite direction (deleting an Author destroys all their Books).*/
            
            var book = await context.Books.FindAsync(id);
    
            if (book == null)
            {
                return NotFound();
            }

            context.Books.Remove(book);
            await context.SaveChangesAsync();

            // Return a 204 No Content response (Standard for a successful delete)
            return NoContent();
        }
    }
}