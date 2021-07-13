using System;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Bug_286_try_to_make_a_singleton_scoped
    {
        [Fact]
        public void cannot_mark_object_as_scoped()
        {
            var container = new Container(x =>
            {
                Should.Throw<InvalidOperationException>(() =>
                {
                    x.For<IWidget>().Use(new AWidget()).Scoped();
                });

            });
            
            
        }
    }
}