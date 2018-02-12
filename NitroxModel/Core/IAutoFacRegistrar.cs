using Autofac;

namespace NitroxModel.Core
{
    public interface IAutoFacRegistrar
    {
        void RegisterDependencies(ContainerBuilder containerBuilder);
    }
}
