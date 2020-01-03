using System;
using System.Threading.Tasks;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lamar.AspNetCoreTests.Integration
{
    public class dbcontext_code_generation
    {
        [Fact]
        public void should_not_be_using_service_provider()
        {
            var container = new Container(x => { 
                x.AddDbContext<InputDbContext>(builder =>
                {
                    builder.UseSqlServer("some connection string");
                    
                }, optionsLifetime:ServiceLifetime.Singleton); 
            });

            var rules = new GenerationRules();
            var source = container.CreateServiceVariableSource();
            var assembly = new GeneratedAssembly(rules);

            var handler = assembly.AddType("SpecialHandler", typeof(Handler));

            var handleMethod = handler.MethodFor(nameof(Handler.Handle));

            var save = MethodCall.For<InputDbContext>(x => x.Add<Input>(null));
            handleMethod.Frames.Add(save);

            var code = assembly.GenerateCode(source);


            code.ShouldNotContain("ServiceProvider");
        }
    }

    public class InputDbContext : DbContext
    {
        public InputDbContext(DbContextOptions<InputDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }

    public abstract class Handler
    {
        public abstract void Handle(Input input);
    }

    public class Input
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}