using System.IO;
using AssetsTools.NET.Extra;

namespace NitroxServer_Subnautica.Resources.Parsers;

public abstract class AssetParser
{
    protected static string rootPath;
    protected static AssetsManager assetsManager;

    static AssetParser()
    {
        rootPath = ResourceAssetsParser.FindDirectoryContainingResourceAssets();
        assetsManager = new AssetsManager();
        assetsManager.LoadClassPackage("classdata.tpk");
        assetsManager.LoadClassDatabaseFromPackage("2019.4.36f1");
        assetsManager.SetMonoTempGenerator(new MonoCecilTempGenerator(Path.Combine(rootPath, "Managed")));
    }

    public static void Dispose()
    {
        assetsManager.UnloadAll(true);
    }
}
