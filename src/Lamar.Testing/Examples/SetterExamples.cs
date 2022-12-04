using System;
using System.Reflection;
using JasperFx.TypeDiscovery;
using Lamar.Scanning.Conventions;
using Shouldly;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget3;
using Xunit;
using Xunit.Abstractions;

namespace Lamar.Testing.Examples;

public interface IDataProvider
{
}

#region sample_setter-injection-with-SetterProperty

public class Repository
{
    // Adding the SetterProperty to a setter directs
    // Lamar to use this property when
    // constructing a Repository instance
    [SetterProperty] public IDataProvider Provider { get; set; }

    [SetterProperty] public bool ShouldCache { get; set; }
}

#endregion

public class DataProvider : IDataProvider
{
}

public class BuildPlans
{
    private readonly ITestOutputHelper _output;

    public BuildPlans(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ShowRepositoryBuildPlan()
    {
        var container = new Container(_ =>
        {
            _.For<IDataProvider>().Use<DataProvider>();
            _.ForConcreteType<Repository>().Configure.Setter<bool>().Is(false);
        });

        _output.WriteLine(container.Model.For<Repository>().Default.DescribeBuildPlan());
    }
}

#region sample_IShippingService

public interface IShippingService
{
    void ShipIt();
}

#endregion

public class ShippingWebService : IShippingService
{
    private readonly string _url;

    public ShippingWebService(string url)
    {
        _url = url;
    }

    public void ShipIt()
    {
        throw new NotImplementedException();
    }
}

public class InternalShippingService : IShippingService
{
    public void ShipIt()
    {
        throw new NotImplementedException();
    }
}

public class ScanningRegistry : ServiceRegistry
{
    public ScanningRegistry()
    {
        Scan(x =>
        {
            // Add assembly by name.
            x.Assembly("Lamar.Testing.Widget");

            // Add an assembly directly
            x.Assembly(typeof(ScanningRegistry).GetTypeInfo().Assembly);

            // Add the assembly that contains a certain type
            x.AssemblyContainingType<IService>();
            // or
            x.AssemblyContainingType(typeof(IService));
        });


        Scan(x =>
        {
            // I'm telling Lamar to sweep a folder called "Extensions" directly
            // underneath the application root folder for any assemblies
            x.AssembliesFromPath("Extensions");

            // I also direct Lamar to add any Registries that it finds in these
            // assemblies.  I'm assuming that all the Lamar directives are
            // contained in Registry classes -- and this is the recommended approach
            x.LookForRegistries();
        });

        Scan(x =>
        {
            // This time I'm going to specify a filter on the assembly such that 
            // only assemblies that have "Extension" in their name will be scanned
            x.AssembliesFromPath("Extensions", assembly => assembly.GetName().Name.Contains("Extension"));

            x.LookForRegistries();
        });
    }
}

#region sample_BasicScanning

public class BasicScanning : ServiceRegistry
{
    public BasicScanning()
    {
        Scan(_ =>
        {
            // Declare which assemblies to scan
            _.Assembly("Lamar.Testing");
            _.AssemblyContainingType<IWidget>();

            // Filter types
            _.Exclude(type => type.Name.Contains("Bad"));

            // A custom registration convention
            _.Convention<MySpecialRegistrationConvention>();

            // Built in registration conventions
            _.AddAllTypesOf<IWidget>().NameBy(x => x.Name.Replace("Widget", ""));
            _.WithDefaultConventions();
        });
    }
}

#endregion

public class MySpecialRegistrationConvention : IRegistrationConvention
{
    public void ScanTypes(TypeSet types, ServiceRegistry registry)
    {
        throw new NotImplementedException();
    }
}

public class Invoice
{
}

public interface IRepository
{
}

public class SimpleRepository : IRepository
{
}

public interface IPresenter
{
    void Activate();
}

public class ShippingScreenPresenter : IPresenter
{
    private readonly IRepository _repository;
    private readonly IShippingService _service;

    #region sample_ShippingScreenPresenter-with-ctor-injection

    // This is the way to write a Constructor Function with an IoC tool
    // Let the IoC container "inject" services from outside, and keep
    // ShippingScreenPresenter ignorant of the IoC infrastructure
    public ShippingScreenPresenter(IShippingService service, IRepository repository)
    {
        _service = service;
        _repository = repository;
    }

    #endregion

    #region sample_ShippingScreenPresenter-anti-pattern

    // This is the wrong way to use an IoC container.  Do NOT invoke the container from
    // the constructor function.  This tightly couples the ShippingScreenPresenter to
    // the IoC container in a harmful way.  This class cannot be used in either
    // production or testing without a valid IoC configuration.  Plus, you're writing more
    // code
    public ShippingScreenPresenter(IContainer container)
    {
        // It's even worse if you use a static facade to retrieve
        // a service locator!
        _service = container.GetInstance<IShippingService>();
        _repository = container.GetInstance<IRepository>();
    }

    #endregion


    #region IPresenter Members

    public void Activate()
    {
    }

    #endregion
}

public class ShowBuildPlanOfShippingPresenter
{
    private readonly ITestOutputHelper _output;

    public ShowBuildPlanOfShippingPresenter(ITestOutputHelper output)
    {
        _output = output;
    }

    #region sample_ShippingScreenPresenter-build-plan

    [Fact]
    public void ShowBuildPlan()
    {
        var container = new Container(_ =>
        {
            _.For<IShippingService>().Use<InternalShippingService>();
            _.For<IRepository>().Use<SimpleRepository>();
        });

        // Just proving that we can build ShippingScreenPresenter;)
        container.GetInstance<ShippingScreenPresenter>().ShouldNotBeNull();

        var buildPlan = container.Model.For<ShippingScreenPresenter>().Default.DescribeBuildPlan();

        // _output is the xUnit ITestOutputHelper here
        _output.WriteLine(buildPlan);
    }

    #endregion
}

public class ApplicationController
{
    public void ActivateScreenFor<T>() where T : IPresenter
    {
        //IPresenter presenter = ObjectFactory.GetInstance<T>();
        //presenter.Activate();
    }

    public void ActivateScreen(IPresenter presenter)
    {
    }
}

public class Navigates
{
}

public interface IEditInvoiceView
{
}

public class EditInvoicePresenter : IPresenter
{
    private readonly Invoice _invoice;
    private readonly IRepository _repository;
    private readonly IEditInvoiceView _view;

    public EditInvoicePresenter(IRepository repository, IEditInvoiceView view, Invoice invoice)
    {
        _repository = repository;
        _view = view;
        _invoice = invoice;
    }

    #region IPresenter Members

    public void Activate()
    {
    }

    #endregion

    private void editInvoice(Invoice invoice, ApplicationController controller)
    {
        //var presenter = ObjectFactory.Container.With(invoice).GetInstance<EditInvoicePresenter>();
        //controller.ActivateScreen(presenter);
    }
}

public interface IApplicationShell
{
}

//IQueryToolBar or IExplorerPane
public interface IQueryToolBar
{
}

public interface IExplorerPane
{
}

public class ApplicationShell : IApplicationShell
{
    public IQueryToolBar QueryToolBar => null;

    public IExplorerPane ExplorerPane => null;
}

public class QueryController
{
    private IQueryToolBar _toolBar;

    public QueryController(IQueryToolBar toolBar)
    {
        _toolBar = toolBar;
    }
}

public class InjectionClass
{
    public InjectionClass()
    {
        // Familiar stuff for the average WinForms or WPF developer
        // Create the main form
        var shell = new ApplicationShell();

        // Put the main form, and some of its children into Lamar
        // where other Controllers and Commands can get to them
        // without being coupled to the main form
        //ObjectFactory.Container.Inject<IApplicationShell>(shell);
        //ObjectFactory.Container.Inject(shell.QueryToolBar);
        //ObjectFactory.Container.Inject(shell.ExplorerPane);
    }
}

public class RemoteService : IService
{
    public void DoSomething()
    {
        throw new NotImplementedException();
    }
}

public class InstanceExampleRegistry : ServiceRegistry
{
    public InstanceExampleRegistry()
    {
        // Shortcut for just specifying "use this type -- with auto wiring"
        For<IService>().Use<RemoteService>();

        // Set the default Instance of a ServiceType
        For<IService>().Use<RemoteService>();

        // Add an additional Instance of a ServiceType
        For<IService>().Use<RemoteService>();
    }
}