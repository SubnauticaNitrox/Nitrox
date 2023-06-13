using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsTools.NET.Texture;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer_Subnautica.Resources.Parsers.Abstract;
using NitroxServer_Subnautica.Resources.Parsers.Helper;

namespace NitroxServer_Subnautica.Resources.Parsers;

public class RandomStartParser : BundleFileParser<RandomStartGenerator>
{
    public RandomStartParser() : base("essentials.unity_0ee8dd89ed55f05bc38a09cc77137d4e.bundle", 0) { }

    public override RandomStartGenerator ParseFile()
    {
        AssetFileInfo assetFile = bundleFile.GetAssetInfo(assetsManager, "RandomStart", AssetClassID.Texture2D);
        AssetTypeValueField textureValueField = assetsManager.GetBaseField(assetFileInst, assetFile);
        TextureFile textureFile = TextureFile.ReadTextureFile(textureValueField);
        byte[] texDat = textureFile.GetTextureData(assetFileInst);
        assetsManager.UnloadAll();

        if (texDat == null || texDat.Length <= 0)
        {
            return null;
        }
        
        Bitmap texture = new(textureFile.m_Width, textureFile.m_Height, textureFile.m_Width * 4, PixelFormat.Format32bppArgb,
                             Marshal.UnsafeAddrOfPinnedArrayElement(texDat, 0));
        texture.RotateFlip(RotateFlipType.RotateNoneFlipY);

        return new RandomStartGenerator(texture);

    }
}
