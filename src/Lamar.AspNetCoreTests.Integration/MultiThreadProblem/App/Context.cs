using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App
{
    public class Context : DbContext
    {
        private readonly SecondContext _secondContext;
        public static InMemoryDatabaseRoot DatabaseRoot => new InMemoryDatabaseRoot();

        public Context(SecondContext secondContext)
        {
            _secondContext = secondContext;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase("db", databaseRoot: DatabaseRoot)
                    .ConfigureWarnings(x => x.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));
            }
        }

        public SecondContext SecondContext { get; set; }

        public int RandomProperty => _secondContext.RandomProperty;

        public DbSet<Book> Book { get; set; }
    }
}
