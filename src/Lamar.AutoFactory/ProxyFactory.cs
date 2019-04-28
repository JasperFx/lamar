using Castle.DynamicProxy;
using Lamar;
using Lamar.AutoFactory;

namespace Lamar.AutoFactory
{
    public class ProxyFactory<TPluginType> where TPluginType : class
    {
        private readonly ProxyGenerator _proxyGenerator;
        private readonly IServiceContext _context;
        private readonly IAutoFactoryConventionProvider _conventionProvider;

        public ProxyFactory(ProxyGenerator proxyGenerator, IServiceContext context, IAutoFactoryConventionProvider conventionProvider)
        {
            _proxyGenerator = proxyGenerator;
            _context = context;
            _conventionProvider = conventionProvider;
        }

        public TPluginType Create()
        {
            var container = _context.GetInstance<IContainer>();

            var interceptor = new FactoryInterceptor(container, _conventionProvider);

            return _proxyGenerator.CreateInterfaceProxyWithoutTarget<TPluginType>(interceptor);
        }
    }
}