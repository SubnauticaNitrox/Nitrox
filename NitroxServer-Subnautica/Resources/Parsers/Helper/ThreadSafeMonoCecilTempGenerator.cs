using System;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace NitroxServer_Subnautica.Resources.Parsers.Helper;

public class ThreadSafeMonoCecilTempGenerator(string managedPath) : IMonoBehaviourTemplateGenerator, IDisposable
{
    private readonly MonoCecilTempGenerator generator = new(managedPath);
    private readonly object locker = new();

    public AssetTypeTemplateField GetTemplateField(
        AssetTypeTemplateField baseField,
        string assemblyName,
        string nameSpace,
        string className,
        UnityVersion unityVersion
    )
    {
        lock (locker)
        {
            return generator.GetTemplateField(baseField, assemblyName, nameSpace, className, unityVersion);
        }
    }

    public void Dispose() => generator?.Dispose();
}
