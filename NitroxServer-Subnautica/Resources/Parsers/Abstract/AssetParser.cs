using System.Collections.Generic;
using System.IO;
using AssetsTools.NET.Extra;
using Mono.Cecil;

namespace NitroxServer_Subnautica.Resources.Parsers;

public abstract class AssetParser
{
    protected static readonly string rootPath;
    protected static readonly AssetsManager assetsManager;
    
    private static readonly MonoCecilTempGenerator monoGenerator;

    static AssetParser()
    {
        rootPath = ResourceAssetsParser.FindDirectoryContainingResourceAssets();
        assetsManager = new AssetsManager();
        assetsManager.LoadClassPackage("classdata.tpk");
        assetsManager.LoadClassDatabaseFromPackage("2019.4.36f1");
        monoGenerator = new MonoCecilTempGenerator(Path.Combine(rootPath, "Managed"));
        assetsManager.SetMonoTempGenerator(monoGenerator);
    }

    public static void Dispose()
    {
        assetsManager.UnloadAll(true);
        foreach (KeyValuePair<string,AssemblyDefinition> pair in monoGenerator.loadedAssemblies)
        {
            pair.Value.Dispose();
        }
    }
}
