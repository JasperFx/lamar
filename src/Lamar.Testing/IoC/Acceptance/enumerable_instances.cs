using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;
using Xunit.Abstractions;

namespace Lamar.Testing.IoC.Acceptance
{
    public class BWidget : AWidget{}

    public class CWidget : AWidget
    {
    }
    public class DefaultWidget : AWidget{}
    
    public class enumerable_instances
    {
        private readonly ITestOutputHelper _output;

        public enumerable_instances(ITestOutputHelper output)
        {
            _output = output;
            
            container = new Container(_ =>
            {
                _.For<IWidget>().Add<AWidget>();
                _.For<IWidget>().Add<BWidget>().Singleton();
                _.For<IWidget>().Add<CWidget>().Scoped();
            });
        }

        private Container container;

        
        [Fact]
        public void honor_lifecycle_in_get_instance()
        {
            var array1 = container.GetInstance<IWidget[]>();
            var array2 = container.GetInstance<IWidget[]>();
            
            array1[0].ShouldNotBeSameAs(array2[0]);
            array1[1].ShouldBeSameAs(array2[1]);
            array1[2].ShouldBeSameAs(array2[2]);
        }
        
        [Fact]
        public void honor_lifecycle_in_scope_get_instance()
        {
            var array1 = container.GetInstance<IWidget[]>();
            var nested = container.GetNestedContainer();
            
            var array2 = nested.GetInstance<IWidget[]>();
            var array3 = nested.GetInstance<IWidget[]>();
            
            array1[0].ShouldNotBeSameAs(array2[0]);
            array1[1].ShouldBeSameAs(array2[1]);
            array1[2].ShouldNotBeSameAs(array2[2]);
            
            array2[0].ShouldNotBeSameAs(array3[0]);
            array2[1].ShouldBeSameAs(array3[1]);
            array2[2].ShouldBeSameAs(array3[2]);
        }
        
        [Fact]
        public void honor_lifecycle_in_get_instance_as_dependency()
        {
            var array1 = container.GetInstance<WidgetArrayHolder>().Widgets;
            var array2 = container.GetInstance<WidgetArrayHolder>().Widgets;
            
            array1[0].ShouldNotBeSameAs(array2[0]);
            array1[1].ShouldBeSameAs(array2[1]);
            array1[2].ShouldBeSameAs(array2[2]);
        }
        
        [Fact]
        public void honor_lifecycle_in_scope_get_instance_as_dependency()
        {
            var array1 = container.GetInstance<WidgetArrayHolder>().Widgets;
            var nested = container.GetNestedContainer();
            
            var array2 = nested.GetInstance<WidgetArrayHolder>().Widgets;
            var array3 = nested.GetInstance<WidgetArrayHolder>().Widgets;
            
            array1[0].ShouldNotBeSameAs(array2[0]);
            array1[1].ShouldBeSameAs(array2[1]);
            array1[2].ShouldNotBeSameAs(array2[2]);
            
            array2[0].ShouldNotBeSameAs(array3[0]);
            array2[1].ShouldBeSameAs(array3[1]);
            array2[2].ShouldBeSameAs(array3[2]);
        }
        
        [Fact]
        public void retrieve_as_ilist()
        {
            // IList<T>
            container.GetInstance<IList<IWidget>>()
                .Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));

        }

        [Fact]
        public void retrieve_as_list()
        {
            container.GetInstance<List<IWidget>>()
                .Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));

        }
        
        [Fact]
        public void retrieve_as_enumerable()
        {
            container.GetInstance<IEnumerable<IWidget>>()
                .Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));

        }

        [Fact]
        public void retrieve_as_collection()
        {
            container.GetInstance<ICollection<IWidget>>()
                     .Select(x => x.GetType())
                     .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));
        }

        [Fact]
        public void retrieve_as_readonlycollection()
        {
            container.GetInstance<IReadOnlyCollection<IWidget>>()
                .Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));
        }

        [Fact]
        public void retrieve_as_array()
        {
            var code = container.Model.For<IWidget[]>().Default.DescribeBuildPlan();
            _output.WriteLine(code);
            
            var widgets = container.GetInstance<IWidget[]>();

            widgets
                .Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));
        }

        public class WidgetArrayUser
        {
            public IWidget[] Widgets { get; }

            public WidgetArrayUser(IWidget[] widgets)
            {
                Widgets = widgets;
            }
        }

        [Fact]
        public void override_array_behavior()
        {
            var container = new Container(_ =>
            {
                _.For<IWidget>().Add<AWidget>();
                _.For<IWidget>().Add<BWidget>();
                _.For<IWidget>().Add<CWidget>();

                _.For<IWidget[]>().Use(new IWidget[] { new DefaultWidget() });
            });

            
            
            container.GetInstance<IWidget[]>()
                .Single().ShouldBeOfType<DefaultWidget>();
        }
        
        [Fact]
        public void override_list_behavior()
        {
            var container = new Container(_ =>
            {
                _.For<IWidget>().Add<AWidget>();
                _.For<IWidget>().Add<BWidget>();
                _.For<IWidget>().Add<CWidget>();

                _.For<IList<IWidget>>().Use(new IWidget[] { new DefaultWidget() }.ToList());
            });

            container.GetInstance<IList<IWidget>>()
                .Single().ShouldBeOfType<DefaultWidget>();
        }

        [Theory]
        [InlineData(typeof(WidgetArrayHolder))]
        [InlineData(typeof(WidgetListHolder))]
        [InlineData(typeof(WidgetIListHolder))]
        [InlineData(typeof(WidgetIEnumerableHolder))]
        [InlineData(typeof(WidgetICollectionHolder))]
        [InlineData(typeof(WidgetIReadOnlyCollectionHolder))]
        public void can_use_as_a_dependency(Type concreteType)
        {
            container.GetInstance(concreteType).As<IWidgetHolder>()
                .Widgets                
                .Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(AWidget), typeof(BWidget), typeof(CWidget));
        }

    }

    public interface IWidgetHolder
    {
        IWidget[] Widgets { get; }
    }

    public class WidgetArrayHolder : IWidgetHolder
    {
        public IWidget[] Widgets { get; }

        public WidgetArrayHolder(IWidget[] widgets)
        {
            Widgets = widgets;
        }
    }
    
    public class WidgetListHolder : IWidgetHolder
    {
        public IWidget[] Widgets { get; }

        public WidgetListHolder(List<IWidget> widgets)
        {
            Widgets = widgets.ToArray();
        }
    }
    
    public class WidgetIListHolder : IWidgetHolder
    {
        public IWidget[] Widgets { get; }

        public WidgetIListHolder(IList<IWidget> widgets)
        {
            Widgets = widgets.ToArray();
        }
    }
    
    public class WidgetIEnumerableHolder : IWidgetHolder
    {
        public IWidget[] Widgets { get; }

        public WidgetIEnumerableHolder(IEnumerable<IWidget> widgets)
        {
            Widgets = widgets.ToArray();
        }
    }

    public class WidgetICollectionHolder : IWidgetHolder
    {
        public IWidget[] Widgets { get; }

        public WidgetICollectionHolder(ICollection<IWidget> widgets)
        {
            Widgets = widgets.ToArray();
        }
    }

    public class WidgetIReadOnlyCollectionHolder : IWidgetHolder
    {
        public IWidget[] Widgets { get; }

        public WidgetIReadOnlyCollectionHolder(IReadOnlyCollection<IWidget> widgets)
        {
            Widgets = widgets.ToArray();
        }
    }
}