﻿using System;
using System.Collections.Generic;
using System.Linq;
using JasperFx.CodeGeneration.Model;
using JasperFx.Core;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Frames;

public class ServiceVariableSource : IServiceVariableSource
{
    public const string UsingNestedContainerDirectly = @"Using the nested container service location approach
because at least one dependency is directly using the Lamar container
or IServiceProvider because of an opaque scoped or transient Lambda registration";

    private readonly IList<InjectedServiceField> _fields = new List<InjectedServiceField>();

    private readonly ServiceGraph _services;
    private readonly IList<ServiceStandinVariable> _standins = new List<ServiceStandinVariable>();
    private Variable _nested = new NestedContainerCreation().Nested;
    private bool _usesNestedContainerDirectly;

    public ServiceVariableSource(ServiceGraph services)
    {
        _services = services;
    }

    public bool Matches(Type type)
    {
        return _services.CouldResolve(type);
    }

    public bool TryFindKeyedService(Type type, string key, out Variable variable)
    {
        variable = default;

        var instance = _services.FindInstance(type, key);

        if (instance == null)
        {
            return false;
        }

        variable = buildPlanForInstance(instance);
        return variable != null;
    }

    public Variable Create(Type type)
    {
        if (type == typeof(IContainer))
        {
            _usesNestedContainerDirectly = true;
            return _nested;
        }

        if (type == typeof(IServiceProvider))
        {
            _usesNestedContainerDirectly = true;
            return new CastVariable(_nested, typeof(IServiceProvider));
        }

        var instance = _services.FindDefault(type);
        return buildPlanForInstance(instance);
    }

    private Variable buildPlanForInstance(Instance instance)
    {
        if (instance.Lifetime == ServiceLifetime.Singleton)
        {
            var field = _fields.FirstOrDefault(x => x.Instance == instance);
            if (field == null)
            {
                field = new InjectedServiceField(instance);
                _fields.Add(field);
            }

            return field;
        }

        var standin = new ServiceStandinVariable(instance);
        _standins.Add(standin);

        return standin;
    }

    public void ReplaceVariables(IMethodVariables method)
    {
        if (_usesNestedContainerDirectly || _standins.Any(x => x.Instance.RequiresServiceProvider(method)))
        {
            useServiceProvider(method);
        }
        else
        {
            useInlineConstruction(method);
        }
    }

    public void StartNewType()
    {
        StartNewMethod();
        _fields.Clear();
    }

    public void StartNewMethod()
    {
        _nested = new NestedContainerCreation().Nested;
        _standins.Clear();
    }

    private void useInlineConstruction(IMethodVariables method)
    {
        // THIS NEEDS TO BE SCOPED PER METHOD!!!
        var variables = new ResolverVariables(method, _fields);
        foreach (var standin in _standins)
        {
            var variable = variables.Resolve(standin.Instance, BuildMode.Inline);
            standin.UseInner(variable);
        }

        variables.OfType<InjectedServiceField>().Each(field =>
        {
            var family = _services.FindAll(field.VariableType);
            field.IsOnlyOne = family.Length == 1;
        });

        variables.MakeNamesUnique();
    }

    private void useServiceProvider(IMethodVariables method)
    {
        var written = false;
        foreach (var standin in _standins)
        {
            var frame = new GetInstanceFromNestedContainerFrame(_nested, standin.VariableType);
            var variable = frame.Variable;

            // Write description of why this had to use the nested container
            if (standin.Instance.RequiresServiceProvider(method))
            {
                var comment = standin.Instance.WhyRequireServiceProvider(method);

                if (_usesNestedContainerDirectly && !written)
                {
                    comment += Environment.NewLine;
                    comment += UsingNestedContainerDirectly;

                    written = true;
                }

                frame.MultiLineComment(comment);
            }
            else if (_usesNestedContainerDirectly && !written)
            {
                frame.MultiLineComment(UsingNestedContainerDirectly);
                written = true;
            }

            standin.UseInner(variable);
        }

        var duplicates = _standins.GroupBy(x => x.Usage).Where(x => x.Count() > 1);
        foreach (var duplicate in duplicates)
        {
            var usage = 0;
            foreach (var standinVariable in duplicate) standinVariable.OverrideName(standinVariable.Usage + ++usage);
        }
    }
}