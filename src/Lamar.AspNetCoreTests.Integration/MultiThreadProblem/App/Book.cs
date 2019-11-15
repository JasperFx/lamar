using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }
    }
}
