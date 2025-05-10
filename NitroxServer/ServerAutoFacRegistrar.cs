global using NitroxModel.Logger;
using System.Reflection;
using Autofac;
using NitroxModel.Core;

namespace NitroxServer
{
    public class ServerAutoFacRegistrar : IAutoFacRegistrar
    {
        public virtual void RegisterDependencies(ContainerBuilder containerBuilder)
        {
        }
    }
}
