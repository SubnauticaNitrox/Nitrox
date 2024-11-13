using System;
using System.Collections.Generic;
using System.Threading;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Mono.Cecil;

namespace NitroxServer_Subnautica.Resources.Parsers.Helper;

public class ThreadSafeMonoCecilTempGenerator : IMonoBehaviourTemplateGenerator, IDisposable
{
    private readonly MonoCecilTempGenerator generator;
    private readonly Lock locker = new();

    public ThreadSafeMonoCecilTempGenerator(string managedPath)
    {
        generator = new MonoCecilTempGenerator(managedPath);
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
