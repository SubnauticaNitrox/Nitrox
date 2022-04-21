using Autofac;

namespace NitroxModel.Core
{
    /// <summary>
    ///     Nitrox projects should inherit from this interface and register their services into the DI container using the builder method.
    /// </summary>
    public interface IAutoFacRegistrar
    {
        void RegisterDependencies(ContainerBuilder containerBuilder);
    }
}
