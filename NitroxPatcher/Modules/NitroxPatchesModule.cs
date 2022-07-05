using System.Reflection;
using Autofac;
using NitroxPatcher.Patches;

namespace NitroxPatcher.Modules;

/// <summary>
///     Simple Dependency Injection (DI) container for registering the patch classes with AutoFac.
/// </summary>
public class NitroxPatchesModule : Autofac.Module
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
