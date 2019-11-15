using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lamar.AspNetCoreTests.Integration.MultiThreadProblem.App
{
    public class OtherService : IOtherService
    {
        private readonly Context _scopedContext;

        public OtherService(Context scopedContext)
        {
            _scopedContext = scopedContext;
        }

        public async Task AddBunchOfBooks()
        {
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(1000);
                _scopedContext.Add(new Book
                {
                    Author = Guid.NewGuid().ToString(),
                    Title = Guid.NewGuid().ToString()
                });
            }

            await _scopedContext.SaveChangesAsync();
        }
    }
}
