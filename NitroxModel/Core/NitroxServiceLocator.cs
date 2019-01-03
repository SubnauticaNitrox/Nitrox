using System;
using Autofac;
using Autofac.Builder;

namespace NitroxModel.Core
{
    public static class NitroxServiceLocator
    {
        public static IContainer DependencyContainer { get; private set; }
        public static ILifetimeScope CurrentLifetimeScope { get; private set; }

        public static void InitializeDependencyContainer(IAutoFacRegistrar dependencyRegistrar)
        {
            ContainerBuilder builder = new ContainerBuilder();
            dependencyRegistrar.RegisterDependencies(builder);

            //IgnoreStartableComponents - we don't want to cause "phantom" executions of the Start() method 
            //on a Monobehaviour because someone accidentally did something funky with a DI registration.
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
            if (CurrentLifetimeScope.IsRegistered<T>())
            {
                return CurrentLifetimeScope.Resolve<T>();
            }
            return null;
        }

        public static object LocateService(Type serviceType)
        {
            CheckServiceResolutionViability();
            if (CurrentLifetimeScope.IsRegistered(serviceType))
            {
                return CurrentLifetimeScope.Resolve(serviceType);
            }
            return null;
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
