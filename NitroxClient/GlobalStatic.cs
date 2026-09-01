using System;
using System.Collections;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Core;
using UWE;

namespace NitroxClient;

internal static class GlobalStatic
{
    /// <summary>
    ///     Starts a coroutine that won't cancel unless the game process exits.
    /// </summary>
    public static void StartCoroutineDetached(IEnumerator coroutine) => CoroutineHost.StartCoroutine(coroutine);

    /// <summary>
    ///     Starts a coroutine that stops automatically when multiplayer ends.
    /// </summary>
    public static bool StartCoroutineMultiplayer(IEnumerator coroutine)
    {
        if (Multiplayer.Main.AliveOrNull() is { } mp)
        {
            mp.StartCoroutine(coroutine);
            return true;
        }
        return false;
    }

    /// <inheritdoc cref="NitroxServicesManager.AddUnityTaskAsync" />
    public static async Task UnityDispatchAsync(Action action) => await NitroxServicesManager.AddUnityTaskAsync(action);
}
