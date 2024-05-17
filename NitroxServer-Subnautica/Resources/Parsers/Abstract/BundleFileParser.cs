using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace NitroxServer_Subnautica.Resources.Parsers.Abstract;

public abstract class BundleFileParser<T> : AssetParser
{
    protected BundleFileInstance bundleFileInstance;
    protected AssetsFileInstance assetFileInst;
    protected AssetsFile bundleFile;

    protected BundleFileParser(string bundleName, int index) : base()
    {
        string bundlePath = Path.Combine(rootPath, "StreamingAssets", "aa", "StandaloneWindows64", bundleName);
        BundleFileInstance bundleFileInst = assetsManager.LoadBundleFile(bundlePath);
        assetFileInst = assetsManager.LoadAssetsFileFromBundle(bundleFileInst, index, loadDeps: true);
        bundleFile = assetFileInst.file;
    }

    protected override void Clear()
    {
        assetsManager.UnloadBundleFile(bundleFileInstance);
        bundleFileInstance = null;
        assetFileInst = null;
        bundleFile = null;
    }

    public abstract T ParseFile();
}
