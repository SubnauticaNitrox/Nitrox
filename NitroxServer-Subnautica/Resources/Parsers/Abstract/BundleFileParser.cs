using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace NitroxServer_Subnautica.Resources.Parsers.Abstract;

public abstract class BundleFileParser<T> : AssetParser
{
    protected static AssetsFileInstance assetFileInst;
    protected static AssetsFile bundleFile;

    protected BundleFileParser(string bundleName, int index)
    {
        string bundlePath = Path.Combine(ResourceAssetsParser.FindDirectoryContainingResourceAssets(), "StreamingAssets", "aa", "StandaloneWindows64", bundleName);
        BundleFileInstance bundleFileInst = assetsManager.LoadBundleFile(bundlePath);
        assetFileInst = assetsManager.LoadAssetsFileFromBundle(bundleFileInst, index, true);
        bundleFile = assetFileInst.file;
    }

    public abstract T ParseFile();

}
