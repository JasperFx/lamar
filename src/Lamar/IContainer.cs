using System;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar
{
    public interface INestedContainer : IServiceContext
    {
        /// <summary>
        /// Inject an object into a container at runtime. Used primarily for services like
        /// HttpContext that are passed into a nested container
        /// </summary>
        /// <param name="object"></param>
        /// <typeparam name="T"></typeparam>
        void Inject<T>(T @object);
    }
    
    public interface IContainer : IServiceContext, IDisposable
    {
        /// <summary>
        /// Starts a "Nested" Container for atomic, isolated access.
        /// </summary>
        /// <returns>The created nested container.</returns>
        INestedContainer GetNestedContainer();

        /// <summary>
        /// Use with caution!  Does a full environment test of the configuration of this container.  Will try to create
        /// every configured instance and afterward calls any methods marked with
        /// <see cref="ValidationMethodAttribute"/>.
        /// </summary>
        void AssertConfigurationIsValid(AssertMode mode = AssertMode.Full);

        /// <summary>
        /// Add additional registrations to a running service. USE WITH CAUTION.
        /// </summary>
        /// <param name="services"></param>
        void Configure(IServiceCollection services);

        /// <summary>
        /// Add additional registrations to a running service. USE WITH CAUTION.
        /// </summary>
        /// <param name="configure"></param>
        void Configure(Action<IServiceCollection> configure);
    }

    public enum AssertMode
    {
        /// <summary>
        /// Only validate on the known configuration of dependencies without trying to build services
        /// </summary>
        ConfigOnly,
        
        /// <summary>
        /// Validate configuration, try to build all services, and execute any environment tests
        /// </summary>
        Full
    }
    
    public enum DisposalLock
    {
        /// <summary>
        /// If a user calls IContainer.Dispose(), ignore the request
        /// </summary>
        Ignore,

        /// <summary>
        /// Default "just dispose the container" behavior
        /// </summary>
        Unlocked,

        /// <summary>
        /// Throws an InvalidOperationException when Dispose() is called
        /// </summary>
        ThrowOnDispose
    }
}