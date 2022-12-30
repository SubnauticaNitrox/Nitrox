using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace NitroxServer_Subnautica.Resources.Parsers.Abstract;

public abstract class ResourceFileParser<T> : AssetParser
{
    protected static readonly AssetsFileInstance resourceInst;
    protected static readonly AssetsFile resourceFile;

    static ResourceFileParser()
    {
        resourceInst = assetsManager.LoadAssetsFile(Path.Combine(rootPath, "resources.assets"), true);
        resourceFile = resourceInst.file;
    }
    public abstract T ParseFile();
}
