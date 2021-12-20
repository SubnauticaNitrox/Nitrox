using Autofac;
using NitroxClient.Debuggers;
using NitroxModel.Core;
using NSubstitute;

namespace NitroxTest
{
    public class TestAutoFacRegistrar : IAutoFacRegistrar
    {
        public void RegisterDependencies(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterInstance(Substitute.For<INetworkDebugger>()).As<INetworkDebugger>().SingleInstance();
        }
    }
}
