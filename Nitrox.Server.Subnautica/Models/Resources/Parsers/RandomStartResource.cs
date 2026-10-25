using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Resources.Core;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

internal sealed class RandomStartResource(SubnauticaAssetsManager assetsManager, IOptions<ServerStartOptions> options) : IGameResource
{
    private readonly SubnauticaAssetsManager assetsManager = assetsManager;
    private readonly IOptions<ServerStartOptions> options = options;
    private readonly TaskCompletionSource<RandomStartGenerator> randomStartGeneratorTcs = new();

    public Task LoadAsync(CancellationToken cancellationToken)
    {
        randomStartGeneratorTcs.TrySetResult(LoadAndGetRandomStartGenerator(cancellationToken));
        return Task.CompletedTask;
    }

    public Task CleanupAsync()
    {
        assetsManager.Dispose();
        return Task.CompletedTask;
    }

    public Task<RandomStartGenerator> GetRandomStartGeneratorAsync() => randomStartGeneratorTcs.Task;

    private RandomStartGenerator? LoadAndGetRandomStartGenerator(CancellationToken cancellationToken = default)
    {
        string bundlePath = Path.Combine(options.Value.GetSubnauticaStandaloneResourcePath(), "essentials.unity_0ee8dd89ed55f05bc38a09cc77137d4e.bundle");
        BundleFileInstance bundleFileInst = assetsManager.LoadBundleFile(bundlePath);
        AssetsFileInstance assetFileInst = assetsManager.LoadAssetsFileFromBundle(bundleFileInst, 0, true);
        cancellationToken.ThrowIfCancellationRequested();

        AssetTypeValueField textureValueField = assetsManager.GetBaseField(assetFileInst, assetFileInst.file.GetAssetInfo(assetsManager, "RandomStart", AssetClassID.Texture2D));
        TextureFile textureFile = TextureFile.ReadTextureFile(textureValueField);
        byte[] texDat = textureFile.GetTextureData(assetFileInst);
        cancellationToken.ThrowIfCancellationRequested();

        if (texDat is not { Length: > 0 })
        {
            return null;
        }

        return new RandomStartGenerator(new PixelProvider(texDat, textureFile.m_Width, textureFile.m_Height));
    }

    /// <summary>
    /// Reads pixels from raw BGRA32 texture data. Rows are flipped to make y = 0 the top row, because unity stores textures bottom up.
    /// </summary>
    private sealed class PixelProvider : RandomStartGenerator.IPixelProvider
    {
        private const int BYTES_PER_PIXEL = 4;
        private readonly byte[] bgraData;
        private readonly int width;
        private readonly int height;

        public PixelProvider(byte[] bgraData, int width, int height)
        {
            Validate.NotNull(bgraData);
            if (bgraData.Length < width * height * BYTES_PER_PIXEL)
            {
                throw new ArgumentException($"Texture data is too small for a {width}x{height} BGRA32 texture", nameof(bgraData));
            }
            this.bgraData = bgraData;
            this.width = width;
            this.height = height;
        }

        private int GetPixelOffset(int x, int y)
        {
            x = Math.Clamp(x, 0, width - 1);
            y = height - 1 - Math.Clamp(y, 0, height - 1);
            return (y * width + x) * BYTES_PER_PIXEL;
        }

        public byte GetRed(int x, int y) => bgraData[GetPixelOffset(x, y) + 2];

        public byte GetGreen(int x, int y) => bgraData[GetPixelOffset(x, y) + 1];

        public byte GetBlue(int x, int y) => bgraData[GetPixelOffset(x, y)];
    }
}
