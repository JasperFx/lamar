using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App
{
    public class SecondContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase("db", databaseRoot: Context.DatabaseRoot)
                    .ConfigureWarnings(x => x.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));
            }
        }

        public int RandomProperty => 5;

        public DbSet<Book> Book { get; set; }
    }
}
