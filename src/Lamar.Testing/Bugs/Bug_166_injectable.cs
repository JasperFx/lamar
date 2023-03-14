using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs;

public class Bug_166_injectable
{
    [Fact]
    public void Lamar_Should_Ok()
    {
        var container = new Container(_ =>
        {
            // This is the ONLY registration you need
            _.Injectable<IExecutionContext>();
        });

        // If you need a value in the root container
        container.Inject<IExecutionContext>(new ExecutionContext1());

        container.GetInstance<IExecutionContext>()
            .ShouldBeOfType<ExecutionContext1>();


        var nested = container.GetNestedContainer();

        // Gotta do this too
        var nestedContext = new ExecutionContext2();
        nested.Inject(typeof(IExecutionContext), nestedContext, true);


        nested.GetInstance<IExecutionContext>()
            .ShouldBeSameAs(nestedContext);
    }

    public interface IExecutionContext
    {
    }

    public class ExecutionContext1 : IExecutionContext
    {
    }

    public class ExecutionContext2 : IExecutionContext
    {
    }
}