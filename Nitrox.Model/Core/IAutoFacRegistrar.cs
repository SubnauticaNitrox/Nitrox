using Autofac;

namespace Nitrox.Model.Core
{
    public interface IAutoFacRegistrar
    {
        void RegisterDependencies(ContainerBuilder containerBuilder);
    }
}
