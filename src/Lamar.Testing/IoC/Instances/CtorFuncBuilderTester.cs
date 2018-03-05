using System;
using System.Reflection;
using Baseline;
using Lamar.IoC.Instances;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Instances
{
    public class CtorFuncBuilderTester
    {
        [Fact]
        public void try_to_build_simple_constructors()
        {
            var constructors = typeof(Gadget).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            (var func1, var funcType1) = CtorFuncBuilder.LambdaTypeFor(typeof(Gadget),constructors[0]);
            (var func2, var funcType2) = CtorFuncBuilder.LambdaTypeFor(typeof(Gadget),constructors[1]);
            (var func3, var funcType3) = CtorFuncBuilder.LambdaTypeFor(typeof(Gadget),constructors[2]);

            func1.As<Func<object>>()().ShouldBeOfType<Gadget>();
            func2.As<Func<string, object>>()("Jon").ShouldBeOfType<Gadget>().Name.ShouldBe("Jon");
            func3.As<Func<string, int, object>>()("Jon", 15).ShouldBeOfType<Gadget>().Age.ShouldBe(15);
        }
        
        [Fact]
        public void dependency_of_internal_is_also_internal()
        {
            var constructors = typeof(GadgetHolder).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            (var func, var funcType) = CtorFuncBuilder.LambdaTypeFor(typeof(GadgetHolder), constructors[0]);
            
            funcType.ShouldBe(typeof(Func<object, IWidget, object>));

            func.As<Func<object, IWidget, object>>()(new Gadget("Blue"), new AWidget())
                .ShouldBeOfType<GadgetHolder>();
        }

        [Fact]
        public void do_not_blow_up_with_ridiculous_number_of_parameters()
        {
            var constructors = typeof(RidiculousParameters).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            (var func1, var funcType1) = CtorFuncBuilder.LambdaTypeFor(typeof(RidiculousParameters),constructors[0]);

            func1.As<Func<IClock,IClock,IClock,IClock,IClock,IClock,IClock,IClock,object>>()(new Clock(), new Clock(), new Clock(), new Clock(), new Clock(), new Clock(), new Clock(), new Clock()).ShouldBeOfType<RidiculousParameters>();
        }
    }

    public interface IGadget
    {
        
    }
    
    internal class GadgetHolder : IGadget
    {
        public Gadget Gadget { get; }
        public IWidget Widget { get; }

        public GadgetHolder(Gadget gadget, IWidget widget)
        {
            Gadget = gadget;
            Widget = widget;
        }
    }

    internal class Gadget
    {
        public string Name { get; }
        public int Age { get; }

        public Gadget()
        {
        }

        internal Gadget(string name)
        {
            Name = name;
        }

        public Gadget(string name, int age)
        {
            Name = name;
            Age = age;
        }
    }

    public class RidiculousParameters : IWidget
    {
        public RidiculousParameters(IClock c1, IClock c2, IClock c3, IClock c4, IClock c5, IClock c6, IClock c7, IClock c8)
        {
        }

        public void DoSomething()
        {
        }
    }
}