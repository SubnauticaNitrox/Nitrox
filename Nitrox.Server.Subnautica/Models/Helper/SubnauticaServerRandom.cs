using System;
using System.Threading;
using Microsoft.Extensions.Options;
using NitroxModel.DataStructures.Unity;
using NitroxServer.Helper;

namespace Nitrox.Server.Subnautica.Models.Helper;

/// <summary>
///     The randomness source for Nitrox Subnautica servers.
/// </summary>
public class SubnauticaServerRandom
{
    private static int instanceCounter;
    private readonly XorRandom random;

    public SubnauticaServerRandom(IOptions<Configuration.SubnauticaServerOptions> optionProvider)
    {
        Configuration.SubnauticaServerOptions options = optionProvider.Value;
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Seed);
        random = new XorRandom(NudgeSeed(options.Seed.GetHashCode()));
    }

    public int NextIntRange(int min, int max) => random.NextIntRange(min, max);

    public float NextFloat() => random.NextFloat();

    public NitroxVector3 NextInsideSphere(float radius) => random.NextInsideSphere(radius);

    /// <summary>
    ///     Returns a different seed from the input, even for the same input.
    /// </summary>
    /// <remarks>
    ///     Used to differ the sequences returned by <see cref="SubnauticaServerRandom" />.
    /// </remarks>
    private int NudgeSeed(int seed)
    {
        unchecked
        {
            seed += Interlocked.Increment(ref instanceCounter);
            return seed;
        }
    }
}
