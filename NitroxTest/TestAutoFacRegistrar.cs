using Autofac;
using Moq;
using NitroxClient.Debuggers;
using NitroxModel.Core;

namespace NitroxTest
{
    public class TestAutoFacRegistrar : IAutoFacRegistrar
    {
        public void RegisterDependencies(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterInstance(new Mock<INetworkDebugger>().Object).As<INetworkDebugger>().SingleInstance();
        }
    }
}
