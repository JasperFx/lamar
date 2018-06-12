using System;
using Shouldly;
using StructureMap.Testing.Acceptance;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class inject_to_scope
    {
        [Fact]
        public void inject_into_root_container()
        {
            var container = new Container(_ =>
            {
                _.Injectable<IWidget>();
            });
            
            var widget = new AWidget();
            container.Inject<IWidget>(widget);
            
            container.GetInstance<IWidget>()
                .ShouldBeSameAs(widget);
            
            container.GetInstance<WidgetUser>()
                .Widget.ShouldBeSameAs(widget);
        }

        [Fact]
        public void inject_scoping_with_nested_container()
        {
            var container = new Container(_ =>
            {
                _.Injectable<IWidget>();
            });
            
            var rootWidget = new AWidget();
            container.Inject<IWidget>(rootWidget);
            
            
            var nestedWidget = new BWidget();

            var nested = container.GetNestedContainer();
            nested.Inject<IWidget>(nestedWidget);
            
            container.GetInstance<IWidget>()
                .ShouldBeSameAs(rootWidget);
            
            nested.GetInstance<IWidget>()
                .ShouldBeSameAs(nestedWidget);
            
        }


        [Fact]
        public void using_injected_service()
        {
            // SAMPLE: container-with-injectable
            var container = new Container(_ =>
            {
                _.Injectable<ExecutionContext>();
            });
            // ENDSAMPLE
            
            // SAMPLE: injecting-context-to-nested
            var context = new ExecutionContext();

            var nested = container.GetNestedContainer();
            nested.Inject(context);
            // ENDSAMPLE


            // SAMPLE: resolving-using-context
            var service = nested.GetInstance<ContextUsingService>();
            service.Context.ShouldBeSameAs(context);
            // ENDSAMPLE
        }
    }

    
    
    // SAMPLE: ContextUsingService
    public class ContextUsingService
    {
        public ExecutionContext Context { get; }

        public ContextUsingService(ExecutionContext context)
        {
            Context = context;
        }
    }
    // ENDSAMPLE

    // SAMPLE: ExecutionContext
    // This class is specific to some kind of short lived 
    // process and lives in a nested container
    public class ExecutionContext
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }
    // ENDSAMPLE
}