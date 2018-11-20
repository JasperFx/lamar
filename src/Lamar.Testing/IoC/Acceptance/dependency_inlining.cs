using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using LamarCompiler;
using LamarCompiler.Frames;
using Microsoft.Extensions.DependencyInjection;
using StructureMap.Testing.GenericWidgets;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class dependency_inlining
    {
        private GeneratedAssembly theAssembly;
        private GeneratedType theType;
        private GeneratedMethod theMethod;
        private string _code;
        private ServiceRegistry theServices = new ServiceRegistry();

        public dependency_inlining()
        {
            theAssembly = new GeneratedAssembly(new GenerationRules("Lamar.Generated"));
            theType = theAssembly.AddType("GeneratedClass", typeof(Message1Handler));
            theMethod = theType.MethodFor("Handle");
        }

        private void includeType<T>()
        {
            var methods = typeof(T).GetMethods()
                .Where(x => x.GetParameters().Any(p => p.ParameterType == typeof(Message1)))
                .Select(method => new MethodCall(typeof(T), method));

            theMethod.Frames.AddRange(methods);


        }

        private string theCode
        {
            get
            {
                if (_code.IsEmpty())
                {
                    var container = new Container(theServices);
                    _code = container.GenerateCodeWithInlineServices(theAssembly);
                }

                return _code;
            }
        }
        
        
        
        
        [Fact]
        public void try_single_handler_no_args()
        {
            includeType<NoArgMethod>();

            theCode.ShouldContain("var noArgMethod = new Lamar.Testing.IoC.Acceptance.NoArgMethod();");
        }

        [Fact]
        public void try_single_handler_one_singleton_arg()
        {
            includeType<SingletonArgMethod>();

            theServices.AddSingleton(new MessageTracker());

            theCode.ShouldContain("public GeneratedClass(Lamar.Testing.IoC.Acceptance.MessageTracker messageTracker)");
            theCode.ShouldContain("var singletonArgMethod = new Lamar.Testing.IoC.Acceptance.SingletonArgMethod(_messageTracker);");
        }

        [Fact]
        public void try_single_handler_two_singleton_arg()
        {
            includeType<MultipleArgMethod>();

            theServices.AddSingleton(new MessageTracker());
            theServices.AddSingleton<IWidget, Widget>();


            theCode.ShouldContain("var multipleArgMethod = new Lamar.Testing.IoC.Acceptance.MultipleArgMethod(_messageTracker, _widget);");
        }

        [Fact]
        public void try_single_handler_two_container_scoped_args()
        {
            includeType<MultipleArgMethod>();

            theServices.AddScoped<MessageTracker>();
            theServices.AddScoped<IWidget, Widget>();


            theCode.ShouldContain("var widget = new Lamar.Testing.IoC.Acceptance.Widget();");
            theCode.ShouldContain("var messageTracker = new Lamar.Testing.IoC.Acceptance.MessageTracker();");
            theCode.ShouldContain("var multipleArgMethod = new Lamar.Testing.IoC.Acceptance.MultipleArgMethod(messageTracker, widget);");
        }

        [Fact]
        public void try_single_handler_two_container_scoped_args_one_disposable()
        {
            includeType<MultipleArgMethod>();

            theServices.AddScoped<MessageTracker>();
            theServices.AddScoped<IWidget, DisposedWidget>();


            theCode.ShouldContain("using (var disposedWidget = new Lamar.Testing.IoC.Acceptance.DisposedWidget())");
            theCode.ShouldContain("var messageTracker = new Lamar.Testing.IoC.Acceptance.MessageTracker();");
            theCode.ShouldContain("var multipleArgMethod = new Lamar.Testing.IoC.Acceptance.MultipleArgMethod(messageTracker, disposedWidget);");
        }

        [Fact]
        public void try_single_handler_two_container_scoped_args_both_disposable()
        {
            includeType<MultipleArgMethod>();

            theServices.AddScoped<MessageTracker, DisposedMessageTracker>();
            theServices.AddScoped<IWidget, DisposedWidget>();


            theCode.ShouldContain("using (var disposedMessageTracker = new Lamar.Testing.IoC.Acceptance.DisposedMessageTracker())");
            theCode.ShouldContain("using (var disposedWidget = new Lamar.Testing.IoC.Acceptance.DisposedWidget())");
            theCode.ShouldContain("var multipleArgMethod = new Lamar.Testing.IoC.Acceptance.MultipleArgMethod(disposedMessageTracker, disposedWidget);");
        }

        [Fact]
        public void try_single_handler_one_transient_arg()
        {
            includeType<SingletonArgMethod>();

            theServices.AddTransient<MessageTracker>();

            theCode.ShouldContain("var messageTracker = new Lamar.Testing.IoC.Acceptance.MessageTracker();");
            theCode.ShouldContain("var singletonArgMethod = new Lamar.Testing.IoC.Acceptance.SingletonArgMethod(messageTracker);");
        }

        [Fact]
        public void try_single_handler_one_transient_arg_that_is_disposable()
        {
            includeType<SingletonArgMethod>();

            theServices.AddTransient<MessageTracker, DisposedMessageTracker>();

            theCode.ShouldContain("using (var disposedMessageTracker = new Lamar.Testing.IoC.Acceptance.DisposedMessageTracker())");
            theCode.ShouldContain("var singletonArgMethod = new Lamar.Testing.IoC.Acceptance.SingletonArgMethod(disposedMessageTracker);");
        }

        [Fact]
        public void multiple_actions_using_the_same_transient()
        {
            includeType<WidgetUser>();
            includeType<MultipleArgMethod>();

            theServices.AddTransient<IWidget, Widget>();
            theServices.AddSingleton(new MessageTracker());

            theCode.ShouldContain("var widget1 = new Lamar.Testing.IoC.Acceptance.Widget();");
            theCode.ShouldContain("var multipleArgMethod = new Lamar.Testing.IoC.Acceptance.MultipleArgMethod(_messageTracker, widget2);");
            theCode.ShouldContain("var widget2 = new Lamar.Testing.IoC.Acceptance.Widget();");
            theCode.ShouldContain("var widgetUser = new Lamar.Testing.IoC.Acceptance.WidgetUser(widget1);");
        }
        
        

        [Fact]
        public void multiple_actions_using_the_same_scoped()
        {
            includeType<WidgetUser>();
            includeType<MultipleArgMethod>();

            theServices.AddScoped<IWidget, Widget>();
            theServices.AddSingleton(new MessageTracker());

            theCode.ShouldContain("var widget = new Lamar.Testing.IoC.Acceptance.Widget();");
            theCode.ShouldContain("var multipleArgMethod = new Lamar.Testing.IoC.Acceptance.MultipleArgMethod(_messageTracker, widget);");
            theCode.ShouldContain("var widgetUser = new Lamar.Testing.IoC.Acceptance.WidgetUser(widget);");
        }

        [Fact]
        public void multiple_actions_using_the_same_disposable_transient()
        {
            includeType<WidgetUser>();
            includeType<MultipleArgMethod>();

            theServices.AddTransient<IWidget, DisposedWidget>();
            theServices.AddSingleton(new MessageTracker());


            theCode.ShouldContain("using (var disposedWidget1 = new Lamar.Testing.IoC.Acceptance.DisposedWidget())");
            theCode.ShouldContain("using (var disposedWidget2 = new Lamar.Testing.IoC.Acceptance.DisposedWidget())");
        }

        [Fact]
        public void multiple_actions_one_cannot_be_reduced()
        {
            var container = new Container(_ =>
            {
                _.AddTransient<IWidget>(s => new AWidget());
                _.AddSingleton(new MessageTracker());
            });

            
            // This is enough to make it not be reduceable
            theServices.AddTransient<IWidget>(s => null);
            theServices.AddSingleton(new MessageTracker());

            includeType<SingletonArgMethod>();
            includeType<WidgetUser>();

            theCode.ShouldContain("using (var serviceScope = _serviceScopeFactory.CreateScope())");
            theCode.ShouldContain("var widgetUser = (Lamar.Testing.IoC.Acceptance.WidgetUser)serviceProvider.GetService(typeof(Lamar.Testing.IoC.Acceptance.WidgetUser));");
            theCode.ShouldContain("var singletonArgMethod = (Lamar.Testing.IoC.Acceptance.SingletonArgMethod)serviceProvider.GetService(typeof(Lamar.Testing.IoC.Acceptance.SingletonArgMethod));");
        }

/*

        [Fact]
        public void use_a_known_variable_in_the_mix()
        {
            includeType<UsesKnownServiceThing>();
            theServices.AddSingleton<IFakeStore, FakeStore>();

            var code = theCode;

            code.ShouldContain("using (var session = _fakeStore.OpenSession())");
            code.ShouldContain("var usesKnownServiceThing = new Lamar.Testing.IoC.Acceptance.UsesKnownServiceThing(session);");
        }

        [Fact]
        public void cannot_reduce_with_middleware_that_cannot_be_reduced()
        {
            includeType<UsesKnownServiceThing>();
            theServices.AddScoped<IFakeStore>(s => null);

            var code = theCode;

            code.ShouldContain("var fakeStore = (Jasper.Testing.FakeStoreTypes.IFakeStore)serviceProvider.GetService(typeof(Jasper.Testing.FakeStoreTypes.IFakeStore));");
        }
        
        */

        [Fact]
        public void can_reduce_with_closed_generic_service_dependency()
        {
            includeType<GenericServiceUsingMethod<string>>();
            theServices.AddSingleton(new MessageTracker());
            theServices.AddTransient(typeof(IService<>), typeof(Service<>));

            theCode.ShouldNotContain(typeof(IServiceScopeFactory).Name);

            theCode.ShouldContain("var genericServiceUsingMethod = new Lamar.Testing.IoC.Acceptance.GenericServiceUsingMethod<string>();");

            theCode.ShouldContain("new Lamar.Testing.IoC.Acceptance.Service<string>(_messageTracker);");

        }

        [Fact]
        public void can_reduce_with_array_dependency()
        {
            includeType<HandlerWithArray>();

            theServices.AddTransient<IWidget, RedWidget>();
            theServices.AddScoped<IWidget, GreenWidget>();
            theServices.AddScoped<IWidget, BlueWidget>();

            theCode.ShouldNotContain(typeof(IServiceScopeFactory).Name);

            theCode.ShouldContain("var widgetArray = new StructureMap.Testing.Widget.IWidget[]{redWidget, greenWidget, blueWidget};");
            theCode.ShouldContain("var handlerWithArray = new Lamar.Testing.IoC.Acceptance.HandlerWithArray(widgetArray);");
        }

        [Fact]
        public void use_registered_array_if_one_is_known()
        {
            includeType<HandlerWithArray>();

            includeType<HandlerWithArray>();
            theServices.AddSingleton<IWidget[]>(new IWidget[] {new BlueWidget(), new GreenWidget(),});

            theCode.ShouldContain("public GeneratedClass(StructureMap.Testing.Widget.IWidget[] widgetArray)");
            theCode.ShouldContain("var handlerWithArray = new Lamar.Testing.IoC.Acceptance.HandlerWithArray(_widgetArray);");
        }

        [Fact]
        public void can_reduce_with_enumerable_dependency()
        {
            includeType<WidgetEnumerableUser>();

            theServices.AddTransient<IWidget, RedWidget>();
            theServices.AddScoped<IWidget, GreenWidget>();
            theServices.AddScoped<IWidget, BlueWidget>();

            theCode.ShouldNotContain(typeof(IServiceScopeFactory).Name);

            theCode.ShouldContain("var widgetList = new System.Collections.Generic.List<StructureMap.Testing.Widget.IWidget>{redWidget, greenWidget, blueWidget};");
        }

        [Fact]
        public void use_registered_enumerable_if_one_is_known()
        {
            includeType<WidgetEnumerableUser>();

            theServices.AddSingleton<IEnumerable<IWidget>>(new IWidget[] {new BlueWidget(), new GreenWidget(),});

            theCode.ShouldContain("public GeneratedClass(System.Collections.Generic.IEnumerable<StructureMap.Testing.Widget.IWidget> widgetIEnumerable)");
            theCode.ShouldContain("var widgetEnumerableUser = new Lamar.Testing.IoC.Acceptance.WidgetEnumerableUser(_widgetIEnumerable);");
        }




        [Fact]
        public void can_reduce_with_list_dependency()
        {
            includeType<WidgetListUser>();

            theServices.AddTransient<IWidget, RedWidget>();
            theServices.AddScoped<IWidget, GreenWidget>();
            theServices.AddScoped<IWidget, BlueWidget>();

            theCode.ShouldNotContain(typeof(IServiceScopeFactory).Name);

            theCode.ShouldContain("var widgetList = new System.Collections.Generic.List<StructureMap.Testing.Widget.IWidget>{redWidget, greenWidget, blueWidget};");
        }

        [Fact]
        public void use_registered_list_if_one_is_known()
        {
            includeType<WidgetListUser>();

            theServices.AddSingleton<List<IWidget>>(new List<IWidget> {new BlueWidget(), new GreenWidget(),});

            theCode.ShouldContain("public GeneratedClass(System.Collections.Generic.List<StructureMap.Testing.Widget.IWidget> widgetList)");
            theCode.ShouldContain("var widgetListUser = new Lamar.Testing.IoC.Acceptance.WidgetListUser(_widgetList);");
        }

        [Fact]
        public void can_reduce_with_IList_dependency()
        {
            includeType<WidgetIListUser>();

            theServices.AddTransient<IWidget, RedWidget>();
            theServices.AddScoped<IWidget, GreenWidget>();
            theServices.AddScoped<IWidget, BlueWidget>();

            theCode.ShouldNotContain(typeof(IServiceScopeFactory).Name);

            theCode.ShouldContain("var widgetList = new System.Collections.Generic.List<StructureMap.Testing.Widget.IWidget>{redWidget, greenWidget, blueWidget};");
        }

        [Fact]
        public void use_registered_Ilist_if_one_is_known()
        {
            includeType<WidgetIListUser>();

            theServices.AddSingleton<IList<IWidget>>(new List<IWidget> {new BlueWidget(), new GreenWidget(),});

            theCode.ShouldContain("public GeneratedClass(System.Collections.Generic.IList<StructureMap.Testing.Widget.IWidget> widgetIList)");
            theCode.ShouldContain("var widgetIListUser = new Lamar.Testing.IoC.Acceptance.WidgetIListUser(_widgetIList);");
        }

        [Fact]
        public void can_reduce_with_IReadOnlyList_dependency()
        {
            includeType<WidgetIReadOnlyListUser>();

            theServices.AddTransient<IWidget, RedWidget>();
            theServices.AddScoped<IWidget, GreenWidget>();
            theServices.AddScoped<IWidget, BlueWidget>();

            theCode.ShouldNotContain(typeof(IServiceScopeFactory).Name);

            theCode.ShouldContain("var widgetList = new System.Collections.Generic.List<StructureMap.Testing.Widget.IWidget>{redWidget, greenWidget, blueWidget};");
        }

        [Fact]
        public void use_registered_IReadOnlyList_if_one_is_known()
        {
            includeType<WidgetIReadOnlyListUser>();

            theServices.AddSingleton<IReadOnlyList<IWidget>>(new List<IWidget> {new BlueWidget(), new GreenWidget(),});

            theCode.ShouldContain("public GeneratedClass(System.Collections.Generic.IReadOnlyList<StructureMap.Testing.Widget.IWidget> widgetIReadOnlyList)");
            theCode.ShouldContain("var widgetIReadOnlyListUser = new Lamar.Testing.IoC.Acceptance.WidgetIReadOnlyListUser(_widgetIReadOnlyList);");
        }
        

    }

    public class Message1
    {
        
    }

    public class Widget : IWidget
    {
        public void DoSomething()
        {
            throw new NotImplementedException();
        }
    }
    
    public abstract class Message1Handler
    {
        public abstract Task Handle(Message1 message);
    }



    public class RedWidget : AWidget
    {

    }
    public class GreenWidget : AWidget{}
    public class BlueWidget : AWidget{}

    public class HandlerWithArray
    {
        public HandlerWithArray(IWidget[] widgets)
        {
        }

        public void Handle(Message1 message)
        {

        }
    }

    /*
    [FakeTransaction]
    public class UsesKnownServiceThing
    {
        public UsesKnownServiceThing(IFakeSession session)
        {
        }

        public void Handle(Message1 message)
        {

        }
    }
    */

    public class WidgetEnumerableUser
    {
        public WidgetEnumerableUser(IEnumerable<IWidget> widgets)
        {
        }

        public void Handle(Message1 message)
        {

        }
    }

    public class WidgetListUser
    {
        public WidgetListUser(List<IWidget> widgets)
        {
        }

        public void Handle(Message1 message)
        {

        }
    }

    public class WidgetIListUser
    {
        public WidgetIListUser(IList<IWidget> widgets)
        {
        }

        public void Handle(Message1 message)
        {

        }
    }

    public class WidgetIReadOnlyListUser
    {
        public WidgetIReadOnlyListUser(IReadOnlyList<IWidget> widgets)
        {
        }

        public void Handle(Message1 message)
        {

        }
    }
    
    public class MessageTracker{}

    public class DisposedMessageTracker : MessageTracker, IDisposable
    {
        public void Dispose()
        {

        }
    }

    public class NoArgMethod
    {
        public void Handle(Message1 message)
        {

        }
    }

    public class SingletonArgMethod
    {
        public SingletonArgMethod(MessageTracker tracker)
        {
        }

        public void Handle(Message1 message)
        {

        }
    }

    public class GenericServiceUsingMethod<T>
    {
        public void Handle(Message1 message, IService<T> service)
        {

        }
    }

    public class Service<T> : IService<T>
    {
        public Service(MessageTracker tracker)
        {
        }
    }



    public class MultipleArgMethod
    {
        public MultipleArgMethod(MessageTracker tracker, IWidget widget)
        {
        }

        public void Handle(Message1 message)
        {

        }
    }

    public class WidgetUser
    {
        public IWidget Widget { get; }

        public WidgetUser(IWidget widget)
        {
            Widget = widget;
        }

        public void Handle(Message1 message)
        {

        }
    }

    public class DisposedWidget : IWidget, IDisposable
    {
        public void Dispose()
        {

        }

        public void DoSomething()
        {
            throw new NotImplementedException();
        }
    }
}