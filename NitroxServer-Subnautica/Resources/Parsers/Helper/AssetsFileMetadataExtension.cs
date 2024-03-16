using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace NitroxServer_Subnautica.Resources.Parsers.Helper;

public static class AssetsFileMetadataExtension
{
    public static AssetFileInfo GetAssetInfo(this AssetsFile assetsFile, AssetsManager assetsManager, string assetName, AssetClassID classID)
    {
        foreach (AssetFileInfo assetInfo in assetsFile.GetAssetsOfType(classID))
        {
            if (AssetHelper.GetAssetNameFast(assetsFile, assetsManager.ClassDatabase, assetInfo).Equals(assetName))
            {
                return assetInfo;
            }
        }
        return null;
    }
}
