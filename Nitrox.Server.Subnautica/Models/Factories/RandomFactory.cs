using System.Runtime.CompilerServices;
using Nitrox.Server.Subnautica.Models.Helper;

namespace Nitrox.Server.Subnautica.Models.Factories;

internal sealed class RandomFactory(IOptions<SubnauticaServerOptions> options)
{
    private readonly IOptions<SubnauticaServerOptions> options = options;

    public static int CreateSeedInt32(string worldSeed, string csFilePath, int seedId = 0) => $"{worldSeed}{csFilePath}:{seedId}".ToMd5HashedInt32();

    public static string GetCsFilePathFromType(Type type)
    {
        string assemblyName = type.Assembly.GetName().Name ?? throw new Exception($"Failed to get assembly from type {type}");
        string nameSpaceStr = type.Namespace ?? throw new Exception($"Namespace for {type} is unknown");
        Span<char> nameSpace = stackalloc char[nameSpaceStr.Length];
        nameSpaceStr.CopyTo(nameSpace);
        nameSpace = nameSpace.Slice(assemblyName.Length + 1);
        nameSpace.Replace('.', '/');
        return $"{assemblyName}/{nameSpace}/{type.Name}.cs";
    }

    /// <param name="seedId">Sets the unique id for this seed based on the calling .NET code file.</param>
    /// <param name="filePath">File path to the calling .NET code file.</param>
    public Random GetDotnetRandom(int seedId = 0, [CallerFilePath] string filePath = "") => new(CreateSeedInt32(options.Value.Seed, filePath, seedId));

    /// <inheritdoc cref="GetDotnetRandom" />
    public XorRandom GetUnityLikeRandom(int seedId = 0, [CallerFilePath] string filePath = "") => new(CreateSeedInt32(options.Value.Seed, filePath, seedId));
}
