using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Scanning.Conventions
{
    public interface IRegistrationConvention
    {
        void ScanTypes(TypeSet types, IServiceCollection services);
    }

}
