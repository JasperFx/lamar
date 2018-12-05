using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Lamar.Testing.Bugs
{
    public class Bug_107_idiots_resolving_lists_of_internals
    {
        private readonly ITestOutputHelper _output;

        public Bug_107_idiots_resolving_lists_of_internals(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void do_not_blow_up_retrieving_a_list()
        {
            var container = Container.For(x =>
            {
                x.AddSingleton(new MyThing());
                x.AddSingleton(new MyThing());
                x.AddSingleton(new MyThing());


            });


            try
            {
                container.GetInstance<IList<MyThing>>()
                    .ShouldNotBeNull();
            }
            catch (Exception)
            {
                var code = container.Model.For<IList<MyThing>>().Default.DescribeBuildPlan();
                _output.WriteLine(code);
                throw;
            }
        }

        internal class MyThing
        {
            
        }
    }
}