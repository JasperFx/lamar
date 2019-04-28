using System.Linq;
using Castle.DynamicProxy;
using Lamar;

namespace Lamar.AutoFactory
{
    public class FactoryInterceptor : IInterceptor
    {
        private readonly IContainer _container;
        private readonly IAutoFactoryConventionProvider _conventionProvider;

        public FactoryInterceptor(IContainer container, IAutoFactoryConventionProvider conventionProvider)
        {
            _container = container;
            _conventionProvider = conventionProvider;
        }

        public void Intercept(IInvocation invocation)
        {
            var methodDefinition = _conventionProvider.GetMethodDefinition(invocation.Method, invocation.Arguments);

            if (methodDefinition == null)
            {
                return;
            }

            switch (methodDefinition.MethodType)
            {
                case AutoFactoryMethodType.GetInstance:
                    invocation.ReturnValue = !string.IsNullOrEmpty(methodDefinition.InstanceName)
                        ? _container.TryGetInstance(methodDefinition.InstanceType, methodDefinition.InstanceName)
                        : _container.TryGetInstance(methodDefinition.InstanceType);
                    break;

                
                case AutoFactoryMethodType.GetNames:
                    invocation.ReturnValue = _container.Model.AllInstances
                        .Where(x => x.ServiceType == methodDefinition.InstanceType)
                        .Select(x => x.Instance.HasExplicitName() ? x.Name : string.Empty)
                        .ToList();
                    break;
            }
        }
    }
}