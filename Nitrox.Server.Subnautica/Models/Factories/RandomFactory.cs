using System.Runtime.CompilerServices;
using Nitrox.Server.Subnautica.Models.Helper;

namespace Nitrox.Server.Subnautica.Models.Factories;

internal sealed class RandomFactory(IOptions<SubnauticaServerOptions> options)
{
    private readonly IOptions<SubnauticaServerOptions> options = options;

    public static int CreateSeedInt32(string worldSeed, string csFilePath, int seedId = 0) => $"{worldSeed}{csFilePath}:{seedId}".ToMd5HashedInt32();

    /// <param name="seedId">Sets the unique id for this seed based on the calling .NET code file.</param>
    /// <param name="filePath">File path to the calling .NET code file.</param>
    public Random GetDotnetRandom(int seedId = 0, [CallerFilePath] string filePath = "") => new(CreateSeedInt32(options.Value.Seed, filePath, seedId));

    /// <inheritdoc cref="GetDotnetRandom" />
    public XorRandom GetUnityLikeRandom(int seedId = 0, [CallerFilePath] string filePath = "") => new(CreateSeedInt32(options.Value.Seed, filePath, seedId));
}
