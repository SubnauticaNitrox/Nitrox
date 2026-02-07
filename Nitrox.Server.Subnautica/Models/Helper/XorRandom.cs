using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Server.Subnautica.Models.Helper;

/// <summary>
///     Aims at replicating UnityEngine.Random's implementation to better match behavior of UWE games.
/// </summary>
/// <remarks>
///     Inspired by <see href="https://gist.github.com/macklinb/a00be6b616cbf20fa95e4227575fe50b" />
/// </remarks>
internal sealed class XorRandom
{
    private const uint MT19937 = 1812433253;
    private uint w;
    private uint x;
    private uint y;
    private uint z;

    /// <summary>
    ///     Initialize Xorshift using a signed integer seed, calculating the state values using the initialization method from
    ///     Mersenne Twister (MT19937)
    ///     https://en.wikipedia.org/wiki/Mersenne_Twister#Initialization
    /// </summary>
    public XorRandom(int seed)
    {
        x = (uint)seed;
        y = MT19937 * x + 1;
        z = MT19937 * y + 1;
        w = MT19937 * z + 1;
    }

    public uint XorShift()
    {
        uint t = x ^ (x << 11);
        x = y;
        y = z;
        z = w;
        return w = w ^ (w >> 19) ^ t ^ (t >> 8);
    }

    /// <summary>
    ///     Alias of base Next/XORShift.
    ///     UnityEngine.Random doesn't have any uint functions so these functions behave exactly like int Random.Range
    /// </summary>
    public uint NextUInt()
    {
        return XorShift();
    }

    /// <returns>
    ///     A random unsigned 32-bit integer value in the range 0 (inclusive) to max (exclusive)
    /// </returns>
    public uint NextUIntMax(uint max)
    {
        if (max == 0)
        {
            return 0;
        }
        return XorShift() % max;
    }

    /// <returns>
    ///     A random unsigned 32-bit integer value in the range min (inclusive) to max (exclusive)
    /// </returns>
    public uint NextUIntRange(uint min, uint max)
    {
        if (max - min == 0)
        {
            return min;
        }
        if (max < min)
        {
            return min - XorShift() % (max + min);
        }
        return min + XorShift() % (max - min);
    }

    /// <returns>
    ///     A random signed 32-bit integer value in the range -2,147,483,648 (inclusive) to 2,147,483,647 (inclusive)
    /// </returns>
    public int NextInt()
    {
        return (int)(XorShift() % int.MaxValue);
    }

    public int NextIntMax(int max)
    {
        return NextInt() % max;
    }

    /// <summary>
    ///     A random signed 32-bit integer value in the range min (inclusive) to max (exclusive)
    /// </summary>
    /// <remarks>
    ///     If you only need to generate positive integers, use NextUIntRange instead
    /// </remarks>
    public int NextIntRange(int min, int max)
    {
        // If max and min are the same, just return min since it will result in a DivideByZeroException
        if (max - min == 0)
        {
            return min;
        }

        // Do operations in Int64 to prevent overflow that might be caused by any of the following operations
        // I'm sure there's a faster/better way to do this and avoid casting, but we prefer equivalence to Unity over performance
        long minLong = min;
        long maxLong = max;
        long r = XorShift();

        // Flip the first operator if the max is lower than the min,
        if (max < min)
        {
            return (int)(minLong - r % (maxLong - minLong));
        }
        return (int)(minLong + r % (maxLong - minLong));
    }

    /// <returns>
    ///     A random floating point between 0.0 and 1.0 (inclusive?)
    /// </returns>
    public float NextFloat()
    {
        return 1.0f - NextFloatRange(0.0f, 1.0f);
    }

    /// <returns>
    ///     A random floating point between min (inclusive) and max (exclusive)
    /// </returns>
    public float NextFloatRange(float min, float max)
    {
        return (min - max) * ((float)(XorShift() << 9) / 0xFFFFFFFF) + max;
    }

    public NitroxVector3 NextInsideSphere(float radius = 1f)
    {
        NitroxVector3 pointInUnitSphere = new NitroxVector3(NextFloat(), NextFloat(), NextFloat()).Normalized;
        return pointInUnitSphere * NextFloatRange(0, radius);
    }
}
