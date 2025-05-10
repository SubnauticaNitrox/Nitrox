using System;
using System.Collections.Generic;
using System.Threading;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.Options;
using Mono.Cecil;

namespace Nitrox.Server.Subnautica.Models.Resources.Helper;

public class ThreadSafeMonoCecilTempGenerator : IMonoBehaviourTemplateGenerator, IDisposable
{
    private readonly MonoCecilTempGenerator generator;
    private readonly Lock locker = new();

    public ThreadSafeMonoCecilTempGenerator(IOptions<Configuration.ServerStartOptions> optionsProvider)
    {
        generator = new MonoCecilTempGenerator(optionsProvider.Value.GetSubnauticaManagedPath());
    }

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
