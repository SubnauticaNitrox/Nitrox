using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsTools.NET.Texture;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
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
        byte[] rawTexDat = textureFile.FillPictureData(assetFileInst);
        byte[] texDat = textureFile.DecodeTextureRaw(rawTexDat);
        assetsManager.UnloadAll();

        if (texDat is not { Length: > 0 })
        {
            return null;
        }

        Image<Bgra32> texture = Image.LoadPixelData<Bgra32>(texDat, textureFile.m_Width, textureFile.m_Height);
        texture.Mutate(x => x.Flip(FlipMode.Vertical));
        return new RandomStartGenerator(new PixelProvider(texture));
    }

    private class PixelProvider : RandomStartGenerator.IPixelProvider
    {
        private readonly Image<Bgra32> texture;

        public PixelProvider(Image<Bgra32> texture)
        {
            Validate.NotNull(texture);
            this.texture = texture;
        }

        public byte GetRed(int x, int y) => texture[x, y].R;

        public byte GetGreen(int x, int y) => texture[x, y].G;

        public byte GetBlue(int x, int y) => texture[x, y].B;
    }
}
