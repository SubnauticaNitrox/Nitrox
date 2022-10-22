using AssetsTools.NET;
using AssetsTools.NET.Extra;
using NitroxServer_Subnautica.Resources.Parsers.Abstract;
using NitroxServer_Subnautica.Resources.Parsers.Helper;

namespace NitroxServer_Subnautica.Resources.Parsers;

public class EntityDistributionsParser : ResourceFileParser<string>
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
