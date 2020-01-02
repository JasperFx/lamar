using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Bug_215_disposable_scoping_issue
    {
        private readonly IContainer _container;

        private ConcurrentBag<MyObject2> _instances;
        private ConcurrentBag<MyObjectNoDisposable> _instancesNotDisosables;

        public Bug_215_disposable_scoping_issue()
        {
            _container = new Container(x =>
            {
                x.For<IMyObject>().Use<MyObject>();
                x.AddTransient(typeof(MyObject2));
                
            });
        }

        [Fact]
        public void should_be_no_duplicates_with_disposable()
        {
            _instances = new ConcurrentBag<MyObject2>();
            var tasks = new List<Task>();
            var taskFactory = new TaskFactory();
            var threadsCount = 15;

            for (var i = 0; i < threadsCount; i++)
            {
                tasks.Add(taskFactory.StartNew(CreateDisposableInstance));
            }

            Task.WaitAll(tasks.ToArray());

            var list = _instances.ToList();
            
            
            
            for (var i = 0; i < threadsCount; i++)
            {
                // Check that there is no duplicates created.
                var sameInstances = list.Where(x => x == list[i]).ToList();
                
                sameInstances.Count.ShouldBe(1);
            }

        }
        
        [Fact]
        public void should_be_no_duplicates_without_disposable()
        {
            _instancesNotDisosables = new ConcurrentBag<MyObjectNoDisposable>();
            var tasks = new List<Task>();
            var taskFactory = new TaskFactory();
            var threadsCount = 15;

            for (var i = 0; i < threadsCount; i++)
            {
                tasks.Add(taskFactory.StartNew(CreateNotDisposableInstance));
            }

            Task.WaitAll(tasks.ToArray());

            var list = _instancesNotDisosables.ToList();
            
            
            
            for (var i = 0; i < threadsCount; i++)
            {
                // Check that there is no duplicates created.
                var sameInstances = list.Where(x => x == list[i]).ToList();
                
                sameInstances.Count.ShouldBe(1);
            }

        }

        private MyObject2 CreateDisposableInstance()
        {
            var instance = _container.GetNestedContainer().GetInstance<MyObject2>();
            _instances.Add(instance);
            return instance;
        }
        
        private MyObjectNoDisposable CreateNotDisposableInstance()
        {
            var instance = _container.GetNestedContainer().GetInstance<MyObjectNoDisposable>();
            _instancesNotDisosables.Add(instance);
            return instance;
        }

        public interface IMyObject
        {

        }

        private interface IMyObject2
        {
        }
        
        private interface IMyObject1
        {
        }


        private class MyObject : IMyObject
        {
            public MyObject()
            {

            }
        }
        
        public class MyObjectNoDisposable : IMyObject1{}

        public class MyObject2 : IMyObject2, IDisposable
        {
            private IMyObject _myObject;

            public MyObject2(IMyObject myObject)
            {
                _myObject = myObject;
            }

            public void Dispose()
            {
            }
        }
    }
}


