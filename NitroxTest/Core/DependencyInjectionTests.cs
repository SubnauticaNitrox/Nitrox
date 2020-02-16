using System;
using System.Reflection;
using Autofac;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Core;

namespace NitroxTest.Core
{
    [TestClass]
    public class DependencyInjectionTests
    {
        [ClassInitialize]
        public static void BeforeAll(TestContext context)
        {
            TestAutoFacRegistrar registrar = new TestAutoFacRegistrar();
            NitroxServiceLocator.InitializeDependencyContainer(registrar);
            NitroxServiceLocator.BeginNewLifetimeScope();
        }

        [TestMethod]
        public void ShouldResolveDependencyPolymorphically()
        {
            IRootDependency polymorphicallyResolvedDependency = NitroxServiceLocator.LocateService<IRootDependency>();

            polymorphicallyResolvedDependency.Should().NotBeNull();
            polymorphicallyResolvedDependency.Should().BeOfType<RootDependency>();
        }

        [TestMethod]
        public void ShouldResolveConcreteType()
        {
            DependencyWithRootDependency directConcreteTypeDependency =
                NitroxServiceLocator.LocateService<DependencyWithRootDependency>();

            directConcreteTypeDependency.Should().NotBeNull();
            directConcreteTypeDependency.RootDependency.Should().NotBeNull();
            directConcreteTypeDependency.RootDependency.Should().BeOfType<RootDependency>();
        }

        [TestMethod]
        public void ShouldResolveGenericDependencies()
        {
            IServicer<ServiceRecipientA>
                servicerA = NitroxServiceLocator.LocateService<IServicer<ServiceRecipientA>>();

            IServicer<ServiceRecipientB>
                servicerB = NitroxServiceLocator.LocateService<IServicer<ServiceRecipientB>>();

            servicerA.Should().NotBeNull();
            servicerA.Should().BeOfType<ServiceAProvider>();

            servicerB.Should().NotBeNull();
            servicerB.Should().BeOfType<ServiceBProvider>();
        }

        [TestMethod]
        public void ShouldResolveGenericDependenciesFromManuallyConstructedTypeInstances()
        {
            // Arrange
            Type servicerInstanceType = typeof(IServicer<>);
            Type recipientAType = typeof(ServiceRecipientA);
            Type recipientBType = typeof(ServiceRecipientB);
            Type servicerAType = servicerInstanceType.MakeGenericType(recipientAType);
            Type servicerBType = servicerInstanceType.MakeGenericType(recipientBType);

            // Act
            IServicer<ServiceRecipientA>
                servicerA = (BaseServiceProvider<ServiceRecipientA>)NitroxServiceLocator.LocateService(servicerAType);

            IServicer<ServiceRecipientB>
                servicerB = (BaseServiceProvider<ServiceRecipientB>)NitroxServiceLocator.LocateService(servicerBType);

            // Assert
            servicerA.Should().NotBeNull();
            servicerA.Should().BeOfType<ServiceAProvider>();

            servicerB.Should().NotBeNull();
            servicerB.Should().BeOfType<ServiceBProvider>();
        }

        private class TestAutoFacRegistrar : IAutoFacRegistrar
        {
            public void RegisterDependencies(ContainerBuilder containerBuilder)
            {
                containerBuilder.RegisterType<RootDependency>().As<IRootDependency>();
                containerBuilder.RegisterType<DependencyWithRootDependency>();

                containerBuilder.RegisterAssemblyTypes(Assembly.GetAssembly(GetType()))
                    .AsClosedTypesOf(typeof(IServicer<>));
            }
        }
    }

    public interface IRootDependency
    {
    }

    public class RootDependency : IRootDependency
    {
    }

    public class DependencyWithRootDependency
    {
        public IRootDependency RootDependency { get; }

        public DependencyWithRootDependency(IRootDependency rootDependency)
        {
            RootDependency = rootDependency;
        }
    }

    public interface IServiced
    {
    }

    public interface IServicer<T>
        where T : IServiced
    {
        void PerformService(T serviced);
    }

    public class ServiceRecipientA : IServiced
    {
    }

    public class ServiceRecipientB : IServiced
    {
    }

    public abstract class BaseServiceProvider<TServiced> : IServicer<TServiced>
        where TServiced : IServiced
    {
        public abstract void PerformService(TServiced serviced);
    }

    public class ServiceAProvider : BaseServiceProvider<ServiceRecipientA>
    {
        public override void PerformService(ServiceRecipientA serviced)
        {
            throw new NotImplementedException();
        }
    }

    public class ServiceBProvider : BaseServiceProvider<ServiceRecipientB>
    {
        public override void PerformService(ServiceRecipientB serviced)
        {
            throw new NotImplementedException();
        }
    }
}
