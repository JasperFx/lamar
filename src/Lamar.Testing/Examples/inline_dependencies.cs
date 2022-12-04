using System;
using System.Collections.Generic;
using System.Linq;
using JasperFx.CodeGeneration.Model;
using Lamar.IoC;
using Lamar.IoC.Frames;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.Examples;

public class inline_dependencies
{
    #region sample_inline-dependencies-value

    [Fact]
    public void inline_usage_of_primitive_constructor_argument()
    {
        var container = new Container(_ =>
        {
            _.For<IWidget>().Use<ColorWidget>()
                .Ctor<string>().Is("Red");
        });

        container.GetInstance<IWidget>()
            .ShouldBeOfType<ColorWidget>()
            .Color.ShouldBe("Red");
    }

    #endregion

    #region sample_inline-dependencies-SimpleRule

    public class SimpleRule : IEventRule
    {
        private readonly IAction _action;
        private readonly ICondition _condition;

        public SimpleRule(ICondition condition, IAction action)
        {
            _condition = condition;
            _action = action;
        }

        public void ProcessEvent(SomeEvent @event)
        {
            if (_condition.Matches(@event))
            {
                _action.PerformWork(@event);
            }
        }
    }

    #endregion

    public class Condition1 : ICondition
    {
        public bool Matches(SomeEvent @event)
        {
            return true;
        }
    }

    public class Condition2 : Condition1
    {
    }

    public class Condition3 : Condition1
    {
    }

    public class Action1 : IAction
    {
        public void PerformWork(SomeEvent @event)
        {
            throw new NotImplementedException();
        }
    }

    public class Action2 : Action1
    {
    }

    public class Action3 : Action1
    {
    }

    #region sample_inline-dependencies-simple-ctor-injection

    public class InlineCtorArgs : ServiceRegistry
    {
        public InlineCtorArgs()
        {
            // Defining args by type
            For<IEventRule>().Use<SimpleRule>()
                .Ctor<ICondition>().Is<Condition1>()
                .Ctor<IAction>().Is<Action1>()
                .Named("One");

            // Pass the explicit values for dependencies
            For<IEventRule>().Use<SimpleRule>()
                .Ctor<ICondition>().Is(new Condition2())
                .Ctor<IAction>().Is(new Action2())
                .Named("Two");

            // Rarely used, but gives you a "do any crazy thing" option
            // Pass in your own Instance object
            For<IEventRule>().Use<SimpleRule>()
                .Ctor<IAction>().Is(new MySpecialActionInstance());
        }

        public class BigCondition : ICondition
        {
            public BigCondition(int number)
            {
            }

            public bool Matches(SomeEvent @event)
            {
                throw new NotImplementedException();
            }
        }

        public class MySpecialActionInstance : Instance
        {
            public MySpecialActionInstance() : base(typeof(IAction), typeof(IAction), ServiceLifetime.Transient)
            {
            }

            public override Func<Scope, object> ToResolver(Scope topScope)
            {
                throw new NotImplementedException();
            }

            public override object Resolve(Scope scope)
            {
                throw new NotImplementedException();
            }

            public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
            {
                throw new NotImplementedException();
            }
        }
    }

    #endregion


    #region sample_inline-dependencies-enumerables

    public class BigRule : IEventRule
    {
        private readonly IEnumerable<IAction> _actions;
        private readonly IEnumerable<ICondition> _conditions;

        public BigRule(IEnumerable<ICondition> conditions, IEnumerable<IAction> actions)
        {
            _conditions = conditions;
            _actions = actions;
        }

        public void ProcessEvent(SomeEvent @event)
        {
            if (_conditions.Any(x => x.Matches(@event)))
            {
                foreach (var action in _actions) action.PerformWork(@event);
            }
        }
    }

    #endregion

    #region sample_inline-dependencies-rule-classes

    public class SomeEvent
    {
    }

    public interface ICondition
    {
        bool Matches(SomeEvent @event);
    }

    public interface IAction
    {
        void PerformWork(SomeEvent @event);
    }

    public interface IEventRule
    {
        void ProcessEvent(SomeEvent @event);
    }

    #endregion

    #region sample_inline-dependencies-ctor-by-name

    public class DualConditionRule : IEventRule
    {
        private readonly IAction _action;
        private readonly ICondition _first;
        private readonly ICondition _second;

        public DualConditionRule(ICondition first, ICondition second, IAction action)
        {
            _first = first;
            _second = second;
            _action = action;
        }

        public void ProcessEvent(SomeEvent @event)
        {
            if (_first.Matches(@event) || _second.Matches(@event))
            {
                _action.PerformWork(@event);
            }
        }
    }

    public class DualConditionRuleRegistry : ServiceRegistry
    {
        public DualConditionRuleRegistry()
        {
            // In this case, because DualConditionRule
            // has two different
            For<IEventRule>().Use<DualConditionRule>()
                .Ctor<ICondition>("first").Is<Condition1>()
                .Ctor<ICondition>("second").Is<Condition2>();
        }
    }

    #endregion

    #region sample_inline-dependencies-setters

    public class RuleWithSetters : IEventRule
    {
        public ICondition Condition { get; set; }
        public IAction Action { get; set; }

        public void ProcessEvent(SomeEvent @event)
        {
            if (Condition.Matches(@event))
            {
                Action.PerformWork(@event);
            }
        }
    }

    public class RuleWithSettersRegistry : ServiceRegistry
    {
        public RuleWithSettersRegistry()
        {
            For<IEventRule>().Use<RuleWithSetters>()
                .Setter<ICondition>().Is<Condition1>()

                // or if you need to specify the property name
                .Setter<IAction>("Action").Is<Action2>();
        }
    }

    #endregion

    #region sample_inline-dependencies-open-types

    public interface IEventRule<TEvent>
    {
        void ProcessEvent(TEvent @event);
    }

    public interface ICondition<TEvent>
    {
        bool Matches(TEvent @event);
    }

    public class Condition1<TEvent> : ICondition<TEvent>
    {
        public bool Matches(TEvent @event)
        {
            throw new NotImplementedException();
        }
    }

    public interface IAction<TEvent>
    {
        void PerformWork(TEvent @event);
    }

    public class Action1<TEvent> : IAction<TEvent>
    {
        public void PerformWork(TEvent @event)
        {
            throw new NotImplementedException();
        }
    }

    public class EventRule<TEvent> : IEventRule<TEvent>
    {
        private readonly IAction<TEvent> _action;
        private readonly ICondition<TEvent> _condition;

        public EventRule(string name, ICondition<TEvent> condition, IAction<TEvent> action)
        {
            Name = name;
            _condition = condition;
            _action = action;
        }

        public string Name { get; }

        public void ProcessEvent(TEvent @event)
        {
            if (_condition.Matches(@event))
            {
                _action.PerformWork(@event);
            }
        }
    }

    #endregion
}