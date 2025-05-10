using NitroxServer.Helper;

namespace NitroxServer.Extensions;

public static class ArrayExtensions
{
    public static T GetXorRandom<T>(this T[] choices, T fallback = default)
    {
        if (choices is not { Length: > 0 })
        {
            return fallback;
        }
        return choices[XorRandom.Shared.NextIntRange(0, choices.Length)];
    }
}
