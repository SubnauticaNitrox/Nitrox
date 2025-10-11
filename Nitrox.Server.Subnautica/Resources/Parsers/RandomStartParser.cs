using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Nitrox.Server.Subnautica.Resources.Parsers.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using Nitrox.Server.Subnautica.Resources.Parsers.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Nitrox.Server.Subnautica.Resources.Parsers;

public sealed class RandomStartParser : BundleFileParser<RandomStartGenerator>
{
    private const string BUNDLE_NAME = "essentials.unity_0ee8dd89ed55f05bc38a09cc77137d4e.bundle";
    private const string RANDOM_START_ASSET_NAME = "RandomStart";

    public RandomStartParser() : base(BUNDLE_NAME, 0) { }

    public override RandomStartGenerator? ParseFile()
    {
        AssetFileInfo assetFile = bundleFile.GetAssetInfo(assetsManager, RANDOM_START_ASSET_NAME, AssetClassID.Texture2D);
        AssetTypeValueField textureValueField = assetsManager.GetBaseField(assetFileInst, assetFile);
        TextureFile textureFile = TextureFile.ReadTextureFile(textureValueField);
        byte[] texDat = textureFile.GetTextureData(assetFileInst);
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
