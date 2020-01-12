using System.Reflection;
using Autofac;
using Module = Autofac.Module;

namespace NitroxModel_Subnautica.Proxies
{
    /// <summary>
    ///     Loads types that are used for calling properties and methods on Subnautica codebase.
    ///     This brings the benefits of static typing and reflection caching.
    /// </summary>
    public class NitroxReflectionProxiesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IReflectionProxy>();
        }
    }
}
