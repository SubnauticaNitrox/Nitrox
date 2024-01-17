using NitroxModel.DataStructures.Unity;

namespace NitroxServer.Helper;

/// <summary>
/// Stolen from <see href="https://gist.github.com/macklinb/a00be6b616cbf20fa95e4227575fe50b" />
/// Aims at replicating UnityEngine.Random's implementation which is really uncommon because its uniform.
/// </summary>
public static class XORRandom
{
    private static uint x;
    private static uint y;
    private static uint z;
    private static uint w;

    private const uint MT19937 = 1812433253;

    /// <summary>
    /// Initialize Xorshift using a signed integer seed, calculating the state values using the initialization method from Mersenne Twister (MT19937)
    /// https://en.wikipedia.org/wiki/Mersenne_Twister#Initialization
    /// </summary>
    public static void InitSeed(int seed)
    {
        x = (uint)seed;
        y = (MT19937 * x + 1);
        z = (MT19937 * y + 1);
        w = (MT19937 * z + 1);
    }

    /// <summary>
    /// Explicitly set the state parameters
    /// </summary>
    public static void InitState(uint initial_x, uint initial_y, uint initial_z, uint initial_w)
    {
        x = initial_x;
        y = initial_y;
        z = initial_x;
        w = initial_w;
    }

    public static uint XORShift()
    {
        uint t = x ^ (x << 11);
        x = y; y = z; z = w;
        return w = w ^ (w >> 19) ^ t ^ (t >> 8);
    }


    /// <summary>
    /// Alias of base Next/XORShift.
    /// UnityEngine.Random doesn't have any uint functions so these functions behave exactly like int Random.Range
    /// </summary>
    public static uint NextUInt()
    {
        return XORShift();
    }

    /// <returns>
    /// A random unsigned 32-bit integer value in the range 0 (inclusive) to max (exclusive)
    /// </returns>
    public static uint NextUIntMax(uint max)
    {
        if (max == 0) return 0;
        return XORShift() % max;
    }

    /// <returns>
    /// A random unsigned 32-bit integer value in the range min (inclusive) to max (exclusive)
    /// </returns>
    public static uint NextUIntRange(uint min, uint max)
    {
        if (max - min == 0) return min;

        if (max < min)
            return min - XORShift() % (max + min);
        else
            return min + XORShift() % (max - min);
    }

    /// <returns>
    /// A random signed 32-bit integer value in the range -2,147,483,648 (inclusive) to 2,147,483,647 (inclusive)
    /// </returns>
    public static int NextInt()
    {
        return (int)(XORShift() % int.MaxValue);
    }

    public static int NextIntMax(int max)
    {
        return NextInt() % max;
    }

    /// <summary>
    /// A random signed 32-bit integer value in the range min (inclusive) to max (exclusive)
    /// </summary>
    /// <remarks>
    /// If you only need to generate positive integers, use NextUIntRange instead
    /// </remarks>
    public static int NextIntRange(int min, int max)
    {
        // If max and min are the same, just return min since it will result in a DivideByZeroException
        if (max - min == 0) return min;

        // Do operations in Int64 to prevent overflow that might be caused by any of the following operations
        // I'm sure there's a faster/better way to do this and avoid casting, but we prefer equivalence to Unity over performance
        long minLong = min;
        long maxLong = max;
        long r = XORShift();

        // Flip the first operator if the max is lower than the min,
        if (max < min)
        {
            return (int)(minLong - r % (maxLong - minLong));
        }
        else
        {
            return (int)(minLong + r % (maxLong - minLong));
        }
    }

    /// <returns>
    /// A random floating point between 0.0 and 1.0 (inclusive?)
    /// </returns>
    public static float NextFloat()
    {
        return 1.0f - NextFloatRange(0.0f, 1.0f);
    }

    /// <returns>
    /// A random floating point between min (inclusive) and max (exclusive) 
    /// </returns>
    public static float NextFloatRange(float min, float max)
    {
        return (min - max) * ((float)(XORShift() << 9) / 0xFFFFFFFF) + max;
    }

    public static NitroxVector3 NextInsideSphere(float radius = 1f)
    {
        NitroxVector3 pointInUnitSphere = new NitroxVector3(NextFloat(), NextFloat(), NextFloat()).Normalized;
        return pointInUnitSphere * NextFloatRange(0, radius);
    }
}
