using System.Reflection;
using Autofac;
using Nitrox.Patcher.Patches;
using Module = Autofac.Module;

namespace Nitrox.Patcher.Modules
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
