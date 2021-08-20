using System;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Instances
{
    /// <summary>
    /// Expression Builder that helps to define child dependencies inline 
    /// </summary>
    public class DependencyExpression<TChild> 
    {
        private readonly ConstructorInstance _instance;
        private readonly string _propertyName;

        internal DependencyExpression(ConstructorInstance instance, string propertyName)
        {
            _instance = instance;
            _propertyName = propertyName;
        }


        /// <summary>
        /// Inline dependency by simple Lambda expression
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public ConstructorInstance Is(Func<IServiceContext, TChild> func)
        {
            var child = LambdaInstance.For(func);
            return Is(child);
        }

        /// <summary>
        /// Inline dependency by Lambda Func that uses IContext
        /// </summary>
        /// <param name="description">User friendly description for diagnostics</param>
        /// <param name="func"></param>
        /// <returns></returns>
        public ConstructorInstance Is(string description, Func<IServiceContext, TChild> func)
        {
            var child = LambdaInstance.For(func);
            child.Description = description;
            return Is(child);
        }

        /// <summary>
        /// Shortcut to set an inline dependency to an Instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public ConstructorInstance Is(Instance instance)
        {
            _instance.AddInline(instance);
            if (_propertyName != null)
            {
                instance.Name = _propertyName;
                instance.InlineIsLimitedToExactNameMatch = true;
            }
            return _instance;
        }

        /// <summary>
        /// Shortcut to set an inline dependency to a designated object
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ConstructorInstance Is(TChild value)
        {
            // TODO -- allow nulls some day, because folks always wanna do crazy
            // stuff with them
            if (value == null) throw new ArgumentNullException(nameof(value));

            return Is(new ObjectInstance(typeof(TChild), value));
        }

        /// <summary>
        /// Set the inline dependency to the named instance of the property type
        /// Used mostly to force an optional Setter property to be filled by
        /// StructureMap        /// </summary>
        /// <param name="instanceKey"></param>
        /// <returns></returns>
        public ConstructorInstance IsNamedInstance(string instanceKey)
        {
            return Is(new ReferencedInstance(typeof(TChild), instanceKey));
        }

        /// <summary>
        /// Shortcut method to define a child dependency inline
        /// </summary>
        /// <typeparam name="TConcreteType"></typeparam>
        /// <returns></returns>
        public ConstructorInstance Is<TConcreteType>() where TConcreteType : TChild
        {
            return Is(new ConstructorInstance<TConcreteType, TChild>(typeof(TChild), ServiceLifetime.Transient));
        }


        /// <summary>
        /// Shortcut method to define a child dependency as null
        /// </summary>
        /// <returns></returns>
        public ConstructorInstance IsNull()
        {
            return Is(new NullInstance(typeof(TChild)));
        }


        /// <summary>
        /// Shortcut method to define a child dependency inline and configure
        /// the child dependency
        /// </summary>
        /// <typeparam name="TConcreteType"></typeparam>
        /// <returns></returns>
        public ConstructorInstance Is<TConcreteType>(Action<ConstructorInstance<TConcreteType, TChild>> configure) where TConcreteType : TChild
        {
            var instance = new ConstructorInstance<TConcreteType, TChild>(typeof(TChild), ServiceLifetime.Transient);
            configure(instance);
            return Is(instance);
        }

        /// <summary>
        /// Use the named Instance of TChild for the inline
        /// dependency here
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ConstructorInstance Named(string name)
        {
            return Is(c => c.GetInstance<TChild>(name));
        }
    }
    
}