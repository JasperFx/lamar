using System;
using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Bug_169_bi_directional_problem_with_uri
    {
        public interface IInjectionIssue { }

        public class InjectionIssue : IInjectionIssue
        {
            public Uri Uri { get; }
            public InjectionIssue(Uri uri)
            {
                Uri = uri;
            }
        }

        [Fact]
        public void work_daddummit()
        {
            IContainer container = new Container(x =>
            {
                //breaks
                x.For<IInjectionIssue>()
                    .Use<InjectionIssue>()
                    .Ctor<Uri>().Is(new Uri("http://localhost:9200"));

            });
            
            container.GetInstance<IInjectionIssue>()
                .ShouldBeOfType<InjectionIssue>()
                .Uri.ShouldBe(new Uri("http://localhost:9200"));
        }
    }
}