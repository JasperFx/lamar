using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App
{
    public interface IBookService
    {
        Task<List<Book>> InsertBooksAndReturn();
    }
}
