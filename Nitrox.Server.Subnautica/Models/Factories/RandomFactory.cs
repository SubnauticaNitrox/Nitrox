using System.Runtime.CompilerServices;
using Nitrox.Server.Subnautica.Models.Helper;

namespace Nitrox.Server.Subnautica.Models.Factories;

internal sealed class RandomFactory(IOptions<SubnauticaServerOptions> options)
{
    private readonly IOptions<SubnauticaServerOptions> options = options;

    /// <param name="seedId">Sets the unique id for this seed based on the calling .NET code file.</param>
    /// <param name="filePath">File path to the calling .NET code file.</param>
    public Random GetDotnetRandom(int seedId = 0, [CallerFilePath] string filePath = "") => new($"{options.Value.Seed}{filePath}:{seedId}".ToMd5HashedInt32());

    /// <inheritdoc cref="GetDotnetRandom" />
    public XorRandom GetUnityLikeRandom(int seedId = 0, [CallerFilePath] string filePath = "") => new($"{options.Value.Seed}{filePath}:{seedId}".ToMd5HashedInt32());
}
