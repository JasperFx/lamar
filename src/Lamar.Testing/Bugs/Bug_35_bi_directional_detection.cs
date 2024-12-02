using System;
using System.Collections.Generic;
using System.Linq;
using JasperFx.Core;
using Xunit;

namespace Lamar.Testing.Bugs;

public class Bug_35_bi_directional_detection
{
    [Fact]
    public void detect_and_no_stack_overflow()
    {
        Exception<InvalidOperationException>.ShouldBeThrownBy(() =>
        {
            var container = new Container(x =>
            {
                x.For<N>().Use(ctx => new N(Enumerable.Empty<If>()));

                x.For<If>().Use<N>();
            });
            var instance = container.GetInstance<If>();
        });
    }

    public interface If
    {
    }

    [JasperFxIgnore]
    public class N : If
    {
        public N(IEnumerable<If> n)
        {
        }
    }
}