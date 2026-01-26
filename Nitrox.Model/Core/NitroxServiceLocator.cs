using System;
using Autofac;
using Autofac.Builder;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Core;

/// <summary>
///     Dependency Injection (DI) class for resolving types as defined in the DI registrar, implementing <see cref="IAutoFacRegistrar" />.
/// </summary>
public static class NitroxServiceLocator
{
    public static Func<Type, object> Locator { get; set; } = type =>
    {
        CheckServiceResolutionViability();
        return CurrentLifetimeScope.Resolve(type);
    };
    public static Func<Type, object> OptionalLocator { get; set; } = type =>
    {
        CheckServiceResolutionViability();
        return CurrentLifetimeScope.ResolveOptional(type);
    };

    private static IContainer DependencyContainer { get; set; }
    private static ILifetimeScope CurrentLifetimeScope { get; set; }
    public static event EventHandler LifetimeScopeEnded;

    public static void InitializeDependencyContainer(params IAutoFacRegistrar[] registrars)
    {
        ContainerBuilder builder = new();
        foreach (IAutoFacRegistrar registrar in registrars)
        {
            registrar.RegisterDependencies(builder);
        }

        // IgnoreStartableComponents - Prevents "phantom" executions of the Start() method
        // on a MonoBehaviour because someone accidentally did something funky with a DI registration.
        DependencyContainer = builder.Build(ContainerBuildOptions.IgnoreStartableComponents);
    }

    /// <summary>
    ///     Starts a new life time scope. A single instance per registered service will be returned while this scope is active.
    ///     Services can scoped to this life time using <see cref="IRegistrationBuilder{TLimit,TActivatorData,TRegistrationStyle}.InstancePerLifetimeScope" />.
    /// </summary>
    /// <remarks>
    ///     A life time scope should be created when the game leaves the main menu and loads a level with multiplayer.
    ///     It should end when the game process unloads the level (e.g. player returns to the main menu).
    /// </remarks>
    public static void BeginNewLifetimeScope()
    {
        if (DependencyContainer == null)
        {
            throw new InvalidOperationException("You must install an Autofac container before initializing a new lifetime scope.");
        }

        // If there's an existing scope, invalidate caches before disposing
        // Should allow us to handle instances of stale refs for issue 2545
        if (CurrentLifetimeScope != null)
        {
            OnLifetimeScopeEnded();
            CurrentLifetimeScope.Dispose();
        }

        CurrentLifetimeScope = DependencyContainer.BeginLifetimeScope();
    }

    /// <summary>
    ///     Ends the life time scoped services that were registered using <see cref="IRegistrationBuilder{TLimit,TActivatorData,TRegistrationStyle}.InstancePerLifetimeScope" />.
    /// </summary>
    public static void EndCurrentLifetimeScope()
    {
        CurrentLifetimeScope?.Dispose();
        OnLifetimeScopeEnded();
    }

    /// <summary>
    ///     Only locates the service in the container, pre-lifetime scope.
    /// </summary>
    public static T LocateServicePreLifetime<T>()
    {
        return DependencyContainer.Resolve<T>();
    }

    /// <summary>
    ///     Retrieves a service which was registered into the DI container. Creates a new instance if required.<br />
    /// </summary>
    /// <remarks>
    ///     This method should not be used if the constructor is available for defining a parameter where its type is the service to inject.
    ///     For Unity monobehaviours the constructor is used by Unity and cannot be used to inject services. In this case, use this method.
    /// </remarks>
    public static T LocateService<T>()
        where T : class
    {
        return (T)Locator(typeof(T));
    }

    /// <summary>
    ///     Non-generic alternative to <see cref="LocateService{T}" />.
    /// </summary>
    public static object LocateService(Type serviceType)
    {
        return Locator(serviceType);
    }

    /// <summary>
    ///     Tries to locate the service if it exists. Can return an <see cref="Optional{T}" /> without a value.
    /// </summary>
    /// <typeparam name="T">Type of service to try to locate.</typeparam>
    /// <returns>Optional that might or might not hold the service instance.</returns>
    public static Optional<T> LocateOptionalService<T>() where T : class
    {
        return Optional<T>.OfNullable((T)OptionalLocator(typeof(T)));
    }

    /// <summary>
    ///     Tries to locate the service if it exists. Can return an <see cref="Optional{T}" /> without a value.
    /// </summary>
    /// <param name="serviceType">Type of service to try to locate.</param>
    /// <returns>Optional that might or might not hold the service instance.</returns>
    public static Optional<object> LocateOptionalService(Type serviceType)
    {
        return Optional.OfNullable(OptionalLocator(serviceType));
    }

    /// <summary>
    ///     Throws if a service is asked for but without a proper life time scope.
    /// </summary>
    private static void CheckServiceResolutionViability()
    {
        if (DependencyContainer == null)
        {
            throw new InvalidOperationException("You must install an Autofac container before resolving dependencies.");
        }
        if (CurrentLifetimeScope == null)
        {
            throw new InvalidOperationException("You must begin a new lifetime scope before resolving dependencies.");
        }
    }

    private static void OnLifetimeScopeEnded() => LifetimeScopeEnded?.Invoke(null, EventArgs.Empty);

    /// <summary>
    ///     Generic static class to cache type with very fast lookups. Only use for singleton types.
    /// </summary>
    /// <typeparam name="T">Type in the cache, should be singleton.</typeparam>
    public static class Cache<T> where T : class
    {
        private static T value;
        public static T Value => value ??= LocateServiceAndRegister();
        public static T ValuePreLifetime => value ??= LocateServicePreLifetimeAndRegister();

        private static T LocateServiceAndRegister()
        {
            LifetimeScopeEnded += Invalidate;
            return LocateService<T>();
        }

        private static T LocateServicePreLifetimeAndRegister()
        {
            LifetimeScopeEnded += Invalidate;
            return LocateServicePreLifetime<T>();
        }

        /// <summary>
        ///     Invalidates the cache for type <see cref="T" />. The next <see cref="Value" /> access will request from <see cref="NitroxServiceLocator" /> again.
        /// </summary>
        private static void Invalidate(object _, EventArgs __)
        {
            value = null;
            LifetimeScopeEnded -= Invalidate;
        }
    }
}
