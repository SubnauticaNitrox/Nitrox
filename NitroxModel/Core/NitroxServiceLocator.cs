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
            CheckContainerService();

            T obj;
            if (DependencyContainer.TryResolve(out obj))
            {
                return obj;
            }
            
            CheckLifeTimeService();
            return CurrentLifetimeScope.Resolve<T>();
        }

        public static object LocateService(Type serviceType)
        {
            CheckContainerService();

            object obj;
            if (DependencyContainer.TryResolve(serviceType, out obj))
            {
                return obj;
            }
            
            CheckLifeTimeService();
            return CurrentLifetimeScope.Resolve(serviceType);
        }

        private static void CheckContainerService()
        {
            if (DependencyContainer == null)
            {
                throw new InvalidOperationException("You must install an Autofac container before resolving dependencies.");
            }
        }

        private static void CheckLifeTimeService()
        {
            if (CurrentLifetimeScope == null)
            {
                throw new InvalidOperationException("Type could not be found. The lifetime scope might have it but it has not been created before resolving dependencies.");
            }
        }
    }
}
