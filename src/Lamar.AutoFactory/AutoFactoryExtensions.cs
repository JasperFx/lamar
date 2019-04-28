using Castle.DynamicProxy;
using System;
using Lamar.IoC.Instances;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;

namespace Lamar.AutoFactory
{
    public static class AutoFactoryExtensions
    {
        private static readonly ProxyGenerator proxyGenerator = new ProxyGenerator();

        public static void CreateFactory<TPluginType>(this ServiceRegistry.InstanceExpression<TPluginType> expression)
            where TPluginType : class
        {
            CreateFactory(expression, new DefaultAutoFactoryConventionProvider());
        }

        public static void CreateFactory<TPluginType>(this ServiceRegistry.InstanceExpression<TPluginType> expression,
            IAutoFactoryConventionProvider conventionProvider)
            where TPluginType : class
        {
            var callback = CreateFactoryCallback<TPluginType>(conventionProvider);

            expression.Use(callback);
        }

        public static void CreateFactory<TPluginType, TConventionProvider>(
            this ServiceRegistry.InstanceExpression<TPluginType> expression)
            where TPluginType : class
            where TConventionProvider : IAutoFactoryConventionProvider
        {
            var callback = CreateFactoryCallback<TPluginType, TConventionProvider>();

            expression.Use(callback);
        }

        private static string GetDescription<TPluginType>() where TPluginType : class
        {
            return "AutoFactory builder for " + typeof(TPluginType).GetFullName();
        }

        private static Func<IServiceContext, TPluginType> CreateFactoryCallback<TPluginType>(IAutoFactoryConventionProvider conventionProvider)
            where TPluginType : class
        {
            return ctxt =>
            {
                var proxyFactory = new ProxyFactory<TPluginType>(proxyGenerator, ctxt, conventionProvider);

                return proxyFactory.Create();
            };
        }

        private static Func<IServiceContext, TPluginType> CreateFactoryCallback<TPluginType, TConventionProvider>()
            where TPluginType : class
            where TConventionProvider : IAutoFactoryConventionProvider
        {
            return ctxt =>
            {
                var conventionProvider = ctxt.GetInstance<TConventionProvider>();

                var proxyFactory = new ProxyFactory<TPluginType>(proxyGenerator, ctxt, conventionProvider);

                return proxyFactory.Create();
            };
        }

        public static bool HasExplicitName(this Instance instance)
        {
            // NOTE: I suspect this is a bit too naive
            return !instance.Name.Equals("default") && !instance.Name.Equals(Variable.DefaultArgName(instance.ImplementationType));
        }
    }
}