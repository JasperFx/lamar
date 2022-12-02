using System;
using System.Collections.Generic;
using System.Linq;
using JasperFx.Core;
using LamarCodeGeneration.Model;

namespace LamarCodeGeneration.Frames;

public abstract class TemplateFrame : SyncFrame
{
    private readonly IList<VariableProxy> _proxies = new List<VariableProxy>();
    private string _template;

    protected abstract string Template();

    protected object Arg<T>()
    {
        var proxy = new VariableProxy(_proxies.Count, typeof(T));
        _proxies.Add(proxy);

        return proxy;
    }

    protected object Arg<T>(string name)
    {
        var proxy = new VariableProxy(_proxies.Count, typeof(T), name);
        _proxies.Add(proxy);

        return proxy;
    }

    public sealed override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        var code = _template;
        foreach (var proxy in _proxies) code = proxy.Substitute(code);

        writer.Write(code);
        Next?.GenerateCode(method, writer);
    }

    public sealed override IEnumerable<Variable> FindVariables(IMethodVariables chain)
    {
        _template = Template();

        return _proxies.Select(x => x.Resolve(chain));
    }
}

public class VariableProxy
{
    private readonly string _name;
    private readonly string _substitution;
    private readonly Type _variableType;

    public VariableProxy(int index, Type variableType)
    {
        Index = index;
        _variableType = variableType;

        _substitution = $"~{index}~";
    }

    public VariableProxy(int index, Type variableType, string name)
    {
        Index = index;
        _variableType = variableType;
        _name = name;
    }

    public Variable Variable { get; private set; }

    public int Index { get; }

    public Variable Resolve(IMethodVariables variables)
    {
        Variable = _name.IsEmpty()
            ? variables.FindVariable(_variableType)
            : variables.FindVariableByName(_variableType, _name);

        return Variable;
    }

    public override string ToString()
    {
        return _substitution;
    }

    public string Substitute(string code)
    {
        return code.Replace(_substitution, Variable.Usage);
    }
}