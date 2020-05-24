using BookStore_API.Contracts;
using BookStore_API.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookStore_API.Services
{
    public class BookRepository : IBookRepository
    {
        private readonly ApplicationDbContext _db;

        public BookRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Create(Book entity)
        {
            await _db.Books.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(Book entity)
        {
            _db.Books.Remove(entity);
            return await Save();
        }

        public async Task<IList<Book>> FindAll()
        {
            var authors = await _db.Books.Include(q => q.Author).ToListAsync();
            return authors;
        }

        public async Task<Book> FindById(int id)
        {
            var authors = await _db.Books.Include(q =>q.Author).FirstOrDefaultAsync(q =>q.Id == id);
            return authors;
        }

        public async Task<bool> IsExists(int id)
        {
            var isExists = await _db.Books.AnyAsync(a => a.Id == id);
            return isExists;
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> UpDate(Book entity)
        {
            _db.Books.Update(entity);
            return await Save();
        }
    }
}
