using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace NitroxServer_Subnautica.Resources.Parsers.Abstract;

public abstract class ResourceFileParser<T> : AssetParser
{
    protected AssetsFileInstance resourceInst;
    protected AssetsFile resourceFile;

    protected ResourceFileParser()
    {
        resourceInst = assetsManager.LoadAssetsFile(Path.Combine(rootPath, "resources.assets"), true);
        resourceFile = resourceInst.file;
    }
    
    protected override void Clear()
    {
        assetsManager.UnloadAssetsFile(resourceInst);
        resourceInst = null;
        resourceFile = null;
    }

    public abstract T ParseFile();
}
