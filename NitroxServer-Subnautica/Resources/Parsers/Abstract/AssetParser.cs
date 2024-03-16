using System;
using System.IO;
using AssetsTools.NET.Extra;
using NitroxModel.Helper;
using NitroxServer_Subnautica.Resources.Parsers.Helper;

namespace NitroxServer_Subnautica.Resources.Parsers;

public abstract class AssetParser : IDisposable
{
    protected AssetsManager assetsManager;
    protected string rootPath;

    protected AssetParser()
    {
        rootPath = ResourceAssetsParser.FindDirectoryContainingResourceAssets();
        ThreadSafeMonoCecilTempGenerator monoGen = new(Path.Combine(rootPath, "Managed"));
        assetsManager = new AssetsManager()
        {
            MonoTempGenerator = monoGen
        };

        string classDataPath = Path.Combine(NitroxUser.LauncherPath ?? ".", "Resources", "classdata.tpk");

        assetsManager.LoadClassPackage(classDataPath);
        assetsManager.LoadClassDatabaseFromPackage("2019.4.36f1");
    }

    protected AssetParser(AssetsManager assetsManager)
    {
        Validate.NotNull(assetsManager);
        this.assetsManager = assetsManager;
    }

    protected virtual void Clear()
    {
        assetsManager.UnloadAll(unloadClassData: false);
    }

    public virtual void Dispose()
    {
        assetsManager.UnloadAll(unloadClassData: true);
        assetsManager = null;
    }
}
