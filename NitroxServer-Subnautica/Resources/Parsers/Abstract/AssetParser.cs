using System.IO;
using AssetsTools.NET.Extra;
using NitroxModel.Helper;
using NitroxServer_Subnautica.Resources.Parsers.Helper;

namespace NitroxServer_Subnautica.Resources.Parsers;

public abstract class AssetParser
{
    protected static readonly string rootPath;
    protected static readonly AssetsManager assetsManager;

    private static readonly ThreadSafeMonoCecilTempGenerator monoGen;

    static AssetParser()
    {
        rootPath = ResourceAssetsParser.FindDirectoryContainingResourceAssets();
        assetsManager = new AssetsManager();
        assetsManager.LoadClassPackage(Path.Combine(NitroxUser.AssetsPath, "Resources", "classdata.tpk"));
        assetsManager.LoadClassDatabaseFromPackage("2019.4.36f1");
        assetsManager.SetMonoTempGenerator(monoGen = new ThreadSafeMonoCecilTempGenerator(Path.Combine(rootPath, "Managed")));
    }

    public static void Dispose()
    {
        assetsManager.UnloadAll(true);
        monoGen.Dispose();
    }
}
