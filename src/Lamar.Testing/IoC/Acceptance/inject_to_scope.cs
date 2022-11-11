using System;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class inject_to_scope
    {
        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public void inject_into_root_container( bool replace )
        {
            var container = new Container(_ =>
            {
                _.Injectable<IWidget>();
            });
            
            var widget = new AWidget();
            container.Inject( typeof(IWidget), widget, replace );
            
            container.GetInstance<IWidget>()
                .ShouldBeSameAs(widget);
            
            container.GetInstance<WidgetUser>()
                .Widget.ShouldBeSameAs(widget);
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public void inject_scoping_with_nested_container( bool replace )
        {
            var container = new Container(_ =>
            {
                _.Injectable<IWidget>();
            });
            
            var rootWidget = new AWidget();
            container.Inject( typeof(IWidget), rootWidget, replace );
                        
            var nestedWidget = new BWidget();

            var nested = container.GetNestedContainer();
            nested.Inject<IWidget>(nestedWidget);
            
            container.GetInstance<IWidget>()
                .ShouldBeSameAs(rootWidget);
            
            nested.GetInstance<IWidget>()
                .ShouldBeSameAs(nestedWidget);
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public void requires_assignable_type( bool replace )
        {
            var container = new Container( _ =>
            {
                _.Injectable<IWidget>();
            });

            Assert.Throws<InvalidOperationException>( () => container.Inject( typeof( IWidget ), new object(), replace ) );
        }

        [Fact]
        public void replace_causes_replacement_of_previous_value_on_subsequent_call()
        {
            var container = new Container( _ =>
            {
                _.Injectable<IWidget>();
            });

            var replaced = new AWidget();
            var widget = new AWidget();
            container.Inject( typeof(IWidget), replaced, false );
            container.Inject( typeof(IWidget), widget, true );

            container.GetInstance<IWidget>()
                .ShouldBeSameAs( widget );

            container.GetInstance<WidgetUser>()
                .Widget.ShouldBeSameAs( widget );
        }

        class Derived : Container
        {
            public Derived( Action<ServiceRegistry> configuration ) : base( configuration ) {}
                
            public Type serviceType;
            public object @object;
            public bool replace;

            public override void Inject( Type serviceType, object @object, bool replace )
            {
                this.serviceType = serviceType;
                this.@object = @object;
                this.replace = replace;
                base.Inject( serviceType, @object, replace );
            }
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public void generic_calls_nongeneric( bool replace )
        {
            var container = new Derived( _ =>
            {
                _.Injectable<IWidget>();
            });

            var widget = new AWidget();
            container.Inject<IWidget>( widget, replace );
                
            container.serviceType.ShouldBeSameAs( typeof(IWidget) );
            container.@object.ShouldBeSameAs( widget );
            container.replace.ShouldBe( replace );
                
            container.GetInstance<IWidget>()
                .ShouldBeSameAs( widget );

            container.GetInstance<WidgetUser>()
                .Widget.ShouldBeSameAs( widget );
        }

        [Fact]
        public void generic_calls_nongeneric_without_replace()
        {
            var container = new Derived( _ =>
            {
                _.Injectable<IWidget>();
            } );

            var widget = new AWidget();
            container.Inject<IWidget>( widget );

            container.serviceType.ShouldBeSameAs( typeof( IWidget ) );
            container.@object.ShouldBeSameAs( widget );
            container.replace.ShouldBeFalse();

            container.GetInstance<IWidget>()
                .ShouldBeSameAs( widget );

            container.GetInstance<WidgetUser>()
                .Widget.ShouldBeSameAs( widget );
        }

        [Fact]
        public void using_injected_service()
        {
            #region sample_container-with-injectable
            var container = new Container(_ =>
            {
                _.Injectable<ExecutionContext>();
            });
            #endregion
            
            #region sample_injecting-context-to-nested
            var context = new ExecutionContext();

            var nested = container.GetNestedContainer();
            nested.Inject(context);
            #endregion


            #region sample_resolving-using-context
            var service = nested.GetInstance<ContextUsingService>();
            service.Context.ShouldBeSameAs(context);
            #endregion
        }
    }

    
    
    #region sample_ContextUsingService
    public class ContextUsingService
    {
        public ExecutionContext Context { get; }

        public ContextUsingService(ExecutionContext context)
        {
            Context = context;
        }
    }
    #endregion

    #region sample_ExecutionContext
    // This class is specific to some kind of short lived 
    // process and lives in a nested container
    public class ExecutionContext
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }
    #endregion
}