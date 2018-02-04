using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Core;
using SimpleInjector;

namespace NitroxTest.Core
{
    [TestClass]
    public class DependencyInjectionTests
    {
        [ClassInitialize]
        public static void BeforeAll(TestContext context)
        {
            TestSimpleInjectorContainerBuilder builder = new TestSimpleInjectorContainerBuilder();
            NitroxServiceLocator.InstallContainer(builder);
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

        private class TestSimpleInjectorContainerBuilder : ISimpleInjectorContainerBuilder
        {
            public Container BuildContainer()
            {
                Container testContainer = new Container();

                testContainer.Register<IRootDependency, RootDependency>();
                testContainer.Register<DependencyWithRootDependency>();
                testContainer.Register<IServicer<ServiceRecipientA>, ServiceAProvider>();
                testContainer.Register<IServicer<ServiceRecipientB>, ServiceBProvider>();

                //We can do this if we can somehow run in 4.0
                //testContainer.Register(typeof(IServicer<>), new[] { typeof(IServicer<>).Assembly });

                testContainer.Verify();

                return testContainer;
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

    public class ServiceAProvider : IServicer<ServiceRecipientA>
    {
        public void PerformService(ServiceRecipientA serviced)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ServiceBProvider : IServicer<ServiceRecipientB>
    {
        public void PerformService(ServiceRecipientB serviced)
        {
            throw new System.NotImplementedException();
        }
    }
}
