﻿using StructureMap.Building.Interception;
using StructureMap.Pipeline;
using StructureMap.TypeRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Xunit;

namespace StructureMap.Testing.Samples.Interception
{
    public class Event_Aggregator_Registration
    {
        #region sample_use_the_event_listener_registration
        [Fact]
        public void use_the_event_listener_registration()
        {
            var container = new Container(x =>
            {
                x.Policies.Interceptors(new EventListenerRegistration());
                x.For<IEventAggregator>().Use<EventAggregator>().Singleton();
            });

            var events = container.GetInstance<IEventAggregator>();
            var listener = container.GetInstance<BooMessageListener>();

            var message = new BooMessage();

            events.SendMessage(message);

            listener.Messages.Single().ShouldBeTheSameAs(message);
        }

        #endregion
    }

    public class BooMessage
    {
    }

    public class BooMessageListener : IListener<BooMessage>
    {
        public readonly IList<BooMessage> Messages = new List<BooMessage>();

        public void Handle(BooMessage message)
        {
            Messages.Add(message);
        }
    }

    #region sample_EventListenerRegistration
    public class EventListenerRegistration : IInterceptorPolicy
    {
        public string Description
        {
            get { return "Adds the constructed object to the EventAggregator"; }
        }

        public IEnumerable<IInterceptor> DetermineInterceptors(Type pluginType, Instance instance)
        {
            if (instance.ReturnedType.FindInterfacesThatClose(typeof(IListener<>)).Any())
            {
                Expression<Action<IContext, object>> register =
                    (c, o) => c.GetInstance<IEventAggregator>().AddListener(o);
                yield return new ActivatorInterceptor<object>(register);
            }
        }
    }

    #endregion

    public interface IListener
    {
    }

    #region sample_IListener<T>
    public interface IListener<T>
    {
        void Handle(T message);
    }

    #endregion

    #region sample_IEventAggregator
    public interface IEventAggregator
    {
        // Sending messages
        void SendMessage<T>(T message);

        void SendMessage<T>() where T : new();

        // Explicit registration
        void AddListener(object listener);

        void RemoveListener(object listener);
    }

    #endregion

    public class EventAggregator : IEventAggregator
    {
        private readonly SynchronizationContext _context;
        private readonly List<object> _listeners = new List<object>();
        private readonly object _locker = new object();

        public EventAggregator(SynchronizationContext context)
        {
            _context = context;
        }

        #region IEventAggregator Members

        public void SendMessage<T>(T message)
        {
            sendAction(() => all().OfType<IListener<T>>().Each(x => x.Handle(message)));
        }

        public void SendMessage<T>() where T : new()
        {
            SendMessage(new T());
        }

        public void AddListener(object listener)
        {
            withinLock(() =>
            {
                if (_listeners.Contains(listener)) return;
                _listeners.Add(listener);
            });
        }

        public void RemoveListener(object listener)
        {
            withinLock(() => _listeners.Remove(listener));
        }

        #endregion IEventAggregator Members

        private object[] all()
        {
            lock (_locker)
            {
                return _listeners.ToArray();
            }
        }

        private void withinLock(Action action)
        {
            lock (_locker)
            {
                action();
            }
        }

        protected virtual void sendAction(Action action)
        {
            _context.Send(state => action(), null);
        }

        public void AddListeners(params object[] listeners)
        {
            foreach (object listener in listeners)
            {
                AddListener(listener);
            }
        }

        public bool HasListener(object listener)
        {
            return _listeners.Contains(listener);
        }

        public void RemoveAllListeners()
        {
            _listeners.Clear();
        }

        public void RemoveAllListeners(Predicate<object> filter)
        {
            _listeners.RemoveAll(filter);
        }
    }

    public class FilteredListener<T> : IListener<T>
    {
        private readonly Action<T> _action;
        private readonly Func<T, bool> _filter;

        public FilteredListener(Func<T, bool> filter, Action<T> action)
        {
            _filter = filter;
            _action = action;
        }

        #region IListener<T> Members

        public void Handle(T message)
        {
            if (_filter(message)) _action(message);
        }

        #endregion IListener<T> Members
    }
}