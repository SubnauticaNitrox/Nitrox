using System.IO;
using System.Runtime.InteropServices;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using NitroxServer.GameLogic;
using NitroxServer_Subnautica.Resources.Parsers.Abstract;
using NitroxServer_Subnautica.Resources.Parsers.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
        
        Image<Bgra32> texture = Image.LoadPixelData<Bgra32>(texDat, textureFile.m_Width, textureFile.m_Height);
        texture.Mutate(x => x.Flip(FlipMode.Vertical));
        return new RandomStartGenerator(texture);
    }
}
