using System;
using Autofac;
using Autofac.Builder;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Core
{
    public static class NitroxServiceLocator
    {
        private static IContainer DependencyContainer { get; set; }
        private static ILifetimeScope CurrentLifetimeScope { get; set; }

        public static void InitializeDependencyContainer(IAutoFacRegistrar dependencyRegistrar)
        {
            ContainerBuilder builder = new ContainerBuilder();
            dependencyRegistrar.RegisterDependencies(builder);

            // IgnoreStartableComponents - Prevents "phantom" executions of the Start() method 
            // on a MonoBehaviour because someone accidentally did something funky with a DI registration.
            DependencyContainer = builder.Build(ContainerBuildOptions.IgnoreStartableComponents);
        }

        public static void BeginNewLifetimeScope()
        {
            if (DependencyContainer == null)
            {
                throw new InvalidOperationException("You must install an Autofac container before initializing a new lifetime scope.");
            }

            CurrentLifetimeScope?.Dispose();
            CurrentLifetimeScope = DependencyContainer.BeginLifetimeScope();
        }

        public static void EndCurrentLifetimeScope()
        {
            CurrentLifetimeScope?.Dispose();
        }

        public static T LocateService<T>()
            where T : class
        {
            CheckServiceResolutionViability();
            return CurrentLifetimeScope.Resolve<T>();
        }

        public static object LocateService(Type serviceType)
        {
            CheckServiceResolutionViability();
            return CurrentLifetimeScope.Resolve(serviceType);
        }

        /// <summary>
        ///     Tries to locate the service if it exists. Can return an <see cref="Optional{T}" /> without a value.
        /// </summary>
        /// <typeparam name="T">Type of service to try to locate.</typeparam>
        /// <returns>Optional that might or might not hold the service instance.</returns>
        public static Optional<T> LocateOptionalService<T>()
        {
            CheckServiceResolutionViability();
            T obj;
            return CurrentLifetimeScope.TryResolve(out obj) ? Optional<T>.Of(obj) : Optional<T>.Empty();
        }

        /// <summary>
        ///     Tries to locate the service if it exists. Can return an <see cref="Optional{T}" /> without a value.
        /// </summary>
        /// <param name="serviceType">Type of service to try to locate.</param>
        /// <returns>Optional that might or might not hold the service instance.</returns>
        public static Optional<object> LocateOptionalService(Type serviceType)
        {
            CheckServiceResolutionViability();
            object obj;
            return CurrentLifetimeScope.TryResolve(serviceType, out obj) ? Optional<object>.Of(obj) : Optional<object>.Empty();
        }

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
    }
}
