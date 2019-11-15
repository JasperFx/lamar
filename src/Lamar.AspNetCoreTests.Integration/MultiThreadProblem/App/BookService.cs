using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App
{
    public class BookService : IBookService
    {
        private readonly IContextFactory _contextFactory;
        private readonly IOtherService _otherService;
        private readonly Context _scopedContext;

        public BookService(IContextFactory contextFactory, Context scopedContext, IOtherService otherService)
        {
            _contextFactory = contextFactory;
            _scopedContext = scopedContext;
            _otherService = otherService;
        }

        public async Task<List<Book>> InsertBooksAndReturn()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 25; i++)
            {
                tasks.Add(CreateBook());
            }

            await Task.WhenAll(tasks);
            await _otherService.AddBunchOfBooks();
            return await _scopedContext.Book.Take(100).ToListAsync();
        }

        private async Task CreateBook()
        {
            using (var context = _contextFactory.CreateNewContext())
            {
                context.Add(new Book {Author = Guid.NewGuid().ToString(), Title = Guid.NewGuid().ToString()});
                await Task.Delay(3000);
                await context.SaveChangesAsync();
            }
        }
    }
}