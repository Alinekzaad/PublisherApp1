using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using PublisherData;
using PublisherDomain;

namespace PubAPI;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Book").WithTags(nameof(Book));

        // GET all books
        group.MapGet("/", async (PubContext db) =>
        {
            return await db.Books.Include(b => b.Author)
                                 .Include(b => b.Cover)
                                 .AsNoTracking().ToListAsync();
        })
        .WithName("GetAllBooks")
        .WithOpenApi();

        // GET a single book by ID
        group.MapGet("/{BookId}", async Task<Results<Ok<Book>, NotFound>> (int bookId, PubContext db) =>
        {
            return await db.Books.Include(b => b.Author)
                                 .Include(b => b.Cover)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(b => b.BookId == bookId)
                is Book book ? TypedResults.Ok(book) : TypedResults.NotFound();
        })
        .WithName("GetBookById")
        .WithOpenApi();

        // POST - Create a new book
        group.MapPost("/", async (Book book, PubContext db) =>
        {
            db.Books.Add(book);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Book/{book.BookId}", book);
        })
        .WithName("CreateBook")
        .WithOpenApi();

        // PUT - Update an existing book
        group.MapPut("/{BookId}", async Task<Results<Ok, NotFound>> (int bookId, Book book, PubContext db) =>
        {
            var affected = await db.Books
                .Where(b => b.BookId == bookId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(b => b.Title, book.Title)
                    .SetProperty(b => b.PublishDate, book.PublishDate)
                    .SetProperty(b => b.BasePrice, book.BasePrice)
                    .SetProperty(b => b.AuthorId, book.AuthorId)
                );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateBook")
        .WithOpenApi();

        // DELETE - Remove a book
        group.MapDelete("/{BookId}", async Task<Results<Ok, NotFound>> (int bookId, PubContext db) =>
        {
            var affected = await db.Books
                .Where(b => b.BookId == bookId)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteBook")
        .WithOpenApi();
    }
}
