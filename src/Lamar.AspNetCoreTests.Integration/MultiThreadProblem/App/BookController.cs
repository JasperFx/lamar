using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App
{
    [Route("[controller]")]
    public class BookController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BookController> _logger;

        public BookController(IBookService bookService, ILogger<BookController> logger)
        {
            _bookService = bookService;
            _logger = logger;
        }

        [HttpGet("InsertAndReturn")]
        public async Task<IActionResult> InsertAndReturn()
        {
            try
            {
                var books = await _bookService.InsertBooksAndReturn();
                return Ok(books);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "error");
                throw;
            }
        }
    }
}
