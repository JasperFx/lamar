namespace Lamar.IoC
{
    public interface IContainerInitializer
    {
        void Initialize(Scope scope);
    }
}