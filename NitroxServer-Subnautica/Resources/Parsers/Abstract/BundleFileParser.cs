using System.IO;
using System.Runtime.InteropServices;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace NitroxServer_Subnautica.Resources.Parsers.Abstract;

public abstract class BundleFileParser<T> : AssetParser
{
    protected static AssetsFileInstance assetFileInst;
    protected static AssetsFile bundleFile;

    protected BundleFileParser(string bundleName, int index)
    {
        string standaloneFolderName = "StandaloneWindows64";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            standaloneFolderName = "StandaloneOSX";
        }
        string bundlePath = Path.Combine(ResourceAssetsParser.FindDirectoryContainingResourceAssets(), "StreamingAssets", "aa", standaloneFolderName, bundleName);
        BundleFileInstance bundleFileInst = assetsManager.LoadBundleFile(bundlePath);
        assetFileInst = assetsManager.LoadAssetsFileFromBundle(bundleFileInst, index, true);
        bundleFile = assetFileInst.file;
    }

    public abstract T ParseFile();

}
