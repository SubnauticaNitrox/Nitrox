using System.Collections.Generic;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Mono.Cecil;

namespace Nitrox.Server.Subnautica.Models.Resources.Core;

internal sealed class ThreadSafeMonoCecilTempGenerator(IOptions<ServerStartOptions> optionsProvider) : IMonoBehaviourTemplateGenerator, IDisposable
{
    private readonly MonoCecilTempGenerator generator = new(optionsProvider.Value.GetSubnauticaManagedPath());
    private readonly Lock locker = new();

    public AssetTypeTemplateField GetTemplateField(
        AssetTypeTemplateField baseField,
        string assemblyName,
        string nameSpace,
        string className,
        UnityVersion unityVersion)
    {
        lock (locker)
        {
            return generator.GetTemplateField(baseField, assemblyName, nameSpace, className, unityVersion);
        }
    }

    public void Dispose()
    {
        foreach (KeyValuePair<string, AssemblyDefinition> pair in generator.loadedAssemblies)
        {
            pair.Value.Dispose();
        }
        generator.loadedAssemblies.Clear();
    }
}
