using System.Reflection;
using Autofac;
using NitroxPatcher.Patches;
using Module = Autofac.Module;

namespace NitroxPatcher.Modules
{
    public class NitroxPatchesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IPersistentPatch>()
                .AsImplementedInterfaces();

            builder
                .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IDynamicPatch>()
                .AsImplementedInterfaces();
        }
    }
}
