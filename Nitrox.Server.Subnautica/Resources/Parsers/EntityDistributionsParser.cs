using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Nitrox.Server.Subnautica.Resources.Parsers.Abstract;
using Nitrox.Server.Subnautica.Resources.Parsers.Helper;

namespace Nitrox.Server.Subnautica.Resources.Parsers;

public sealed class EntityDistributionsParser : ResourceFileParser<string>
{
    public override string ParseFile()
    {
        AssetFileInfo assetFileInfo = resourceFile.GetAssetInfo(assetsManager, "EntityDistributions", AssetClassID.TextAsset);
        AssetTypeValueField assetValue = assetsManager.GetBaseField(resourceInst, assetFileInfo);
        string json = assetValue["m_Script"].AsString;
        
        assetsManager.UnloadAll();
        return json;
    }
}
