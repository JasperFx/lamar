using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core;
using Shouldly;
using Xunit;

namespace CodegenTests.Codegen;

public class Loader
{
    public T Load<T>(string id)
    {
        return default;
    }
}

public class MethodCall_generate_code
{
    public readonly GeneratedMethod theMethod = GeneratedMethod.ForNoArg("Foo");

    protected string[] WriteMethod(Expression<Action<MethodTarget>> expression, Action<MethodCall> configure = null)
    {
        var call = MethodCall.For(expression);
        call.Target = Variable.For<MethodTarget>("target");
        configure?.Invoke(call);

        var writer = new SourceWriter();
        call.GenerateCode(theMethod, writer);

        return writer.Code().ReadLines().ToArray();
    }

    [Fact]
    public void use_generic_argument()
    {
        var call = MethodCall.For<Loader>(x => x.Load<string>("foo"));
        call.Target = Variable.For<Loader>("loader");
        call.Arguments[0] = Variable.For<string>("x");

        var writer = new SourceWriter();
        call.GenerateCode(theMethod, writer);

        var code = writer.Code().ReadLines().ToArray();

        code[0].ShouldBe("var result_of_Load = loader.Load<string>(x);");
    }

    [Fact]
    public void use_generic_argument_with_inner()
    {
        var call = MethodCall.For<Loader>(x => x.Load<MyInnerClass>("foo"));
        call.Target = Variable.For<Loader>("loader");
        call.Arguments[0] = Variable.For<string>("x");

        var writer = new SourceWriter();
        call.GenerateCode(theMethod, writer);

        var code = writer.Code().ReadLines().ToArray();

        code[0].ShouldBe(
            "var myInnerClass = loader.Load<CodegenTests.Codegen.MethodCall_generate_code.MyInnerClass>(x);");
    }


    [Fact]
    public void no_return_values_no_arguments()
    {
        WriteMethod(x => x.Go()).Single()
            .ShouldBe("target.Go();");
    }

    [Fact]
    public void no_target_when_local()
    {
        WriteMethod(x => x.Go(), x => x.IsLocal = true)
            .Single().ShouldBe("Go();");
    }

    [Fact]
    public void call_a_sync_generic_method()
    {
        WriteMethod(x => x.Go<string>()).Single()
            .ShouldBe("target.Go<string>();");
    }

    [Fact]
    public void call_a_sync_generic_method_with_multiple_arguments()
    {
        WriteMethod(x => x.Go<string, int, bool>()).Single()
            .ShouldBe("target.Go<string, int, bool>();");
    }

    [Fact]
    public void multiple_arguments()
    {
        WriteMethod(x => x.GoMultiple(null, null, null), x =>
            {
                x.TrySetArgument(Variable.For<Arg1>());
                x.TrySetArgument(Variable.For<Arg2>());
                x.TrySetArgument(Variable.For<Arg3>());
            }).Single()
            .ShouldBe("target.GoMultiple(arg1, arg2, arg3);");
    }

    [Fact]
    public void return_a_value_from_sync_method()
    {
        WriteMethod(x => x.Add(1, 2), x =>
            {
                x.Arguments[0] = Variable.For<int>("x");
                x.Arguments[1] = Variable.For<int>("y");
            }).Single()
            .ShouldBe("var result_of_Add = target.Add(x, y);");
    }

    [Fact]
    public void return_non_simple_value()
    {
        WriteMethod(x => x.Other(null, null), x =>
            {
                x.Arguments[0] = Variable.For<Arg1>();
                x.Arguments[1] = Variable.For<Arg2>();
            }).Single()
            .ShouldBe("var arg3 = target.Other(arg1, arg2);");
    }


    [Fact]
    public void return_task_as_return_from_last_node()
    {
        theMethod.AsyncMode = AsyncMode.ReturnFromLastNode;
        WriteMethod(x => x.GoAsync())
            .Single()
            .ShouldBe("return target.GoAsync();");
    }

    [Fact]
    public void return_task_as_async()
    {
        theMethod.AsyncMode = AsyncMode.AsyncTask;
        WriteMethod(x => x.GoAsync())
            .Single()
            .ShouldBe("await target.GoAsync().ConfigureAwait(false);");
    }


    [Fact]
    public void return_task_as_async_with_configure_await_in_full_framework()
    {
        theMethod.AsyncMode = AsyncMode.AsyncTask;
        WriteMethod(x => x.GoAsync())
            .Single()
            .ShouldBe("await target.GoAsync().ConfigureAwait(false);");
    }


    [Fact]
    public void return_async_value_with_return_from_last_node()
    {
        theMethod.AsyncMode = AsyncMode.ReturnFromLastNode;
        WriteMethod(x => x.OtherAsync(null, null), x =>
            {
                x.Arguments[0] = Variable.For<Arg2>();
                x.Arguments[1] = Variable.For<Arg3>();
            }).Single()
            .ShouldBe("return target.OtherAsync(arg2, arg3);");
    }

    [Fact]
    public void return_async_value_with_async_task()
    {
        theMethod.AsyncMode = AsyncMode.AsyncTask;
        WriteMethod(x => x.OtherAsync(null, null), x =>
            {
                x.Arguments[0] = Variable.For<Arg2>();
                x.Arguments[1] = Variable.For<Arg3>();
            }).Single()
            .ShouldBe("var arg1 = await target.OtherAsync(arg2, arg3).ConfigureAwait(false);");
    }

    [Fact]
    public void disposable_return_value_on_sync_and_disposal_using()
    {
        var lines = WriteMethod(x => x.GetDisposable());
        lines[0].ShouldBe("using var disposableThing = target.GetDisposable();");
    }

    [Fact]
    public void disposable_return_value_on_sync_and_asyncdisposal_using()
    {
        theMethod.AsyncMode = AsyncMode.AsyncTask;
        var lines = WriteMethod(x => x.GetAsyncDisposable());
        lines[0].ShouldBe("await using var asyncDisposableThing = target.GetAsyncDisposable();");
    }

    [Fact]
    public void disposable_return_value_on_sync_no_disposal()
    {
        WriteMethod(x => x.GetDisposable(), x => x.DisposalMode = DisposalMode.None)
            .Single()
            .ShouldBe("var disposableThing = target.GetDisposable();");
    }

    [Fact]
    public void asyncdisposable_return_value_on_sync_no_disposal()
    {
        WriteMethod(x => x.GetAsyncDisposable(), x => x.DisposalMode = DisposalMode.None)
            .Single()
            .ShouldBe("var asyncDisposableThing = target.GetAsyncDisposable();");
    }

    [Fact]
    public void async_disposable_return_from_last_node()
    {
        theMethod.AsyncMode = AsyncMode.ReturnFromLastNode;
        var lines = WriteMethod(x => x.AsyncDisposable());
        lines
            .Single()
            .ShouldBe("return target.AsyncDisposable();");
    }

    [Fact]
    public void async_disposable_async_task()
    {
        theMethod.AsyncMode = AsyncMode.AsyncTask;
        var lines = WriteMethod(x => x.AsyncDisposable());

        lines[0].ShouldBe("using var disposableThing = await target.AsyncDisposable().ConfigureAwait(false);");
    }

    [Fact]
    public void async_disposable_async_task_no_dispose()
    {
        theMethod.AsyncMode = AsyncMode.AsyncTask;
        WriteMethod(x => x.AsyncDisposable(), x => x.DisposalMode = DisposalMode.None)
            .Single()
            .ShouldBe("var disposableThing = await target.AsyncDisposable().ConfigureAwait(false);");
    }


    [Fact]
    public void generate_code_for_assigning_variable_sync()
    {
        Variable assign = Variable.For<int>("number");
        var code = WriteMethod(x => x.Add(0, 0), m =>
        {
            m.AssignResultTo(assign);
            m.Arguments[0] = Variable.For<int>("x");
            m.Arguments[1] = Variable.For<int>("y");

            m.ReturnAction = ReturnAction.Assign;
        });

        code[0].ShouldBe("number = target.Add(x, y);");
    }

    [Fact]
    public void generate_code_for_returning_variable_sync()
    {
        Variable assign = Variable.For<int>("number");
        var code = WriteMethod(x => x.Add(0, 0), m =>
        {
            m.ReturnAction = ReturnAction.Return;
            m.Arguments[0] = Variable.For<int>("x");
            m.Arguments[1] = Variable.For<int>("y");
        });

        code[0].ShouldBe("return target.Add(x, y);");
    }

    [Fact]
    public void generate_code_for_assigning_variable_async()
    {
        Variable assign = Variable.For<int>("number");
        var code = WriteMethod(x => x.AddAsync(0, 0), m =>
        {
            m.AssignResultTo(assign);
            m.Arguments[0] = Variable.For<int>("x");
            m.Arguments[1] = Variable.For<int>("y");
        });

        code[0].ShouldBe("number = await target.AddAsync(x, y).ConfigureAwait(false);");
    }

    [Fact]
    public void generate_code_for_returning_variable_async()
    {
        Variable assign = Variable.For<int>("number");
        var code = WriteMethod(x => x.AddAsync(0, 0), m =>
        {
            m.ReturnAction = ReturnAction.Return;
            m.Arguments[0] = Variable.For<int>("x");
            m.Arguments[1] = Variable.For<int>("y");
        });

        code[0].ShouldBe("return await target.AddAsync(x, y).ConfigureAwait(false);");
    }

    [Fact]
    public void write_code_for_method_returning_ValueTask()
    {
        var code = WriteMethod(x => x.DoValueTask());

        code[0].ShouldBe("await target.DoValueTask();");
    }

    [Fact]
    public void write_code_for_method_returning_ValueTask_from_last_method()
    {
        theMethod.AsyncMode = AsyncMode.ReturnFromLastNode;
        var code = WriteMethod(x => x.DoValueTask());

        code[0].ShouldBe("return target.DoValueTask().AsTask();");
    }

    [Fact]
    public void write_code_for_method_returning_ValueTask_of_string()
    {
        var code = WriteMethod(x => x.ReturnStringFromValueTask());

        code[0].ShouldBe("var result_of_ReturnStringFromValueTask = await target.ReturnStringFromValueTask();");
    }

    [Fact]
    public void write_code_for_method_returning_ValueTask_of_string_when_returning_from_last_node()
    {
        theMethod.AsyncMode = AsyncMode.ReturnFromLastNode;
        var code = WriteMethod(x => x.ReturnStringFromValueTask());

        code[0].ShouldBe("return target.ReturnStringFromValueTask().AsTask();");
    }

    public class MyInnerClass
    {
    }

#if !NET461 && !NET48
    [Fact]
    public void generate_code_for_a_method_that_returns_a_tuple()
    {
        var usage = WriteMethod(x => x.ReturnTuple())
            .First();
        usage.ShouldContain("(var red, var blue, var green) = target.ReturnTuple();");
        usage.ShouldNotContain("var (var red, var blue, var green) = target.ReturnTuple();");
    }

    [Fact]
    public void generate_code_for_a_method_that_returns_a_task_of_tuple_as_await()
    {
        theMethod.AsyncMode = AsyncMode.AsyncTask;
        var usage = WriteMethod(x => x.AsyncReturnTuple())
            .First();
        usage.ShouldContain("(var red, var blue, var green) = await target.AsyncReturnTuple().ConfigureAwait(false);");
        usage.ShouldNotContain(
            "var (var red, var blue, var green) = await target.AsyncReturnTuple().ConfigureAwait(false);");
    }
#endif
}

public class Blue
{
}

public class Green
{
}

public class Red
{
}

public class MethodTarget
{
    public (Red, Blue, Green) ReturnTuple()
    {
        return (new Red(), new Blue(), new Green());
    }

    public Task<(Red, Blue, Green)> AsyncReturnTuple()
    {
        return Task.FromResult((new Red(), new Blue(), new Green()));
    }

    public Task<DisposableThing> AsyncDisposable()
    {
        return Task.FromResult(new DisposableThing());
    }

    public DisposableThing GetDisposable()
    {
        return new DisposableThing();
    }

    public ValueTask DoValueTask()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask<string> ReturnStringFromValueTask()
    {
        return new ValueTask<string>("foo");
    }

    public AsyncDisposableThing GetAsyncDisposable()
    {
        return new AsyncDisposableThing();
    }


    public Task<Arg1> OtherAsync(Arg2 two, Arg3 three)
    {
        return null;
    }

    public Task GoAsync()
    {
        return Task.CompletedTask;
    }

    public Arg3 Other(Arg2 two, Arg3 three)
    {
        return null;
    }

    public int Add(int x, int y)
    {
        return x + y;
    }

    public Task<int> AddAsync(int x, int y)
    {
        return Task.FromResult(x + y);
    }

    public void Go()
    {
    }

    public void GoMultiple(Arg1 one, Arg2 two, Arg3 three)
    {
    }

    public void Go<T>()
    {
    }

    public void Go<T, T1, T2>()
    {
    }

    public class AsyncDisposableThing : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}

public class Arg1
{
}

public class Arg2
{
}

public class Arg3
{
}

public class DisposableThing : IDisposable
{
    public void Dispose()
    {
    }
}