using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JasperFx.Core;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace LamarCodeGeneration;

public interface IGeneratedMethod
{
    FramesCollection Frames { get; }
    Argument[] Arguments { get; }
    IList<Variable> DerivedVariables { get; }
    IList<IVariableSource> Sources { get; }
}

public class GeneratedMethod : IGeneratedMethod
{
    private AsyncMode _asyncMode = AsyncMode.None;
    private readonly MethodInfo _parentMethod;
    private Frame _top;

    public GeneratedMethod(MethodInfo method)
    {
        _parentMethod = method;
        ReturnType = method.ReturnType;
        Arguments = method.GetParameters().Select(x => new Argument(x)).ToArray();
        MethodName = method.Name;

        Frames = new FramesCollection(this);
    }

    public GeneratedMethod(string methodName, Type returnType, params Argument[] arguments)
    {
        ReturnType = returnType;
        Arguments = arguments;
        MethodName = methodName;

        Frames = new FramesCollection(this);
    }

    /// <summary>
    ///     The return type of the method being generated
    /// </summary>
    public Type ReturnType { get; }

    /// <summary>
    ///     The name of the method being generated
    /// </summary>
    public string MethodName { get; }

    public bool Overrides { get; set; }

    /// <summary>
    ///     Is the method synchronous, returning a Task, or an async method
    /// </summary>
    public AsyncMode AsyncMode
    {
        get => _asyncMode;
        set => _asyncMode = value;
    }

    public Variable ReturnVariable { get; set; }

    public GeneratedType ParentType { get; set; }

    public Argument[] Arguments { get; }


    // TODO -- need a test here. It's used within Jasper, but still
    public IList<Variable> DerivedVariables { get; } = new List<Variable>();


    public IList<IVariableSource> Sources { get; } = new List<IVariableSource>();

    public FramesCollection Frames { get; }

    public static GeneratedMethod For<TReturn>(string name, params Argument[] arguments)
    {
        return new GeneratedMethod(name, typeof(TReturn), arguments);
    }

    public static GeneratedMethod ForNoArg(string name)
    {
        return new GeneratedMethod(name, typeof(void));
    }

    public static GeneratedMethod ForNoArg<TReturn>(string name)
    {
        return new GeneratedMethod(name, typeof(TReturn));
    }

    public override string ToString()
    {
        if (Arguments?.Any() ?? false)
        {
            return $"{MethodName}({Arguments.Select(x => x.Declaration).Join(", ")})";
        }

        return $"{MethodName}()";
    }

    public bool WillGenerate()
    {
        if (_parentMethod != null && _parentMethod.IsVirtual && !Frames.Any())
        {
            return false;
        }

        return true;
    }


    public void WriteMethod(ISourceWriter writer)
    {
        if (_top == null)
        {
            throw new InvalidOperationException(
                $"You must call {nameof(ArrangeFrames)}() before writing out the source code");
        }

        var returnValue = determineReturnExpression();

        if (Overrides)
        {
            returnValue = "override " + returnValue;
        }

        var arguments = Arguments.Select(x => x.Declaration).Join(", ");

        writer.Write($"BLOCK:public {returnValue} {MethodName}({arguments})");


        _top.GenerateCode(this, writer);

        writeReturnStatement(writer);

        writer.FinishBlock();
    }

    protected void writeReturnStatement(ISourceWriter writer)
    {
        if (ReturnVariable != null)
        {
            writer.Write($"return {ReturnVariable.Usage};");
        }
        else if ((AsyncMode == AsyncMode.ReturnCompletedTask || AsyncMode == AsyncMode.None) &&
                 ReturnType == typeof(Task))
        {
            writer.Write($"return {typeof(Task).FullNameInCode()}.CompletedTask;");
        }
    }


    protected string determineReturnExpression()
    {
        return AsyncMode == AsyncMode.AsyncTask
            ? "async " + ReturnType.FullNameInCode()
            : ReturnType.FullNameInCode();
    }

    public void ArrangeFrames(GeneratedType type, IServiceVariableSource services)
    {
        if (!Frames.Any())
        {
            throw new ArgumentOutOfRangeException(nameof(Frames), "Cannot be an empty list");
        }

        services?.StartNewMethod();

        var compiler = new MethodFrameArranger(this, type, services);
        try
        {
            compiler.Arrange(out _asyncMode, out _top);
        }
        catch (UnResolvableVariableException e)
        {
            e.Type = type;
            throw;
        }
    }

    public string ToExitStatement()
    {
        return AsyncMode == AsyncMode.AsyncTask
            ? "return;"
            : $"return {typeof(Task).FullName}.{nameof(Task.CompletedTask)};";
    }

    /// <summary>
    ///     Add a return frame for the method's return type
    /// </summary>
    public void Return()
    {
        Frames.Return(ReturnType);
    }
}