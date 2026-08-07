using System.Reflection;
using NitroxClient.MonoBehaviours.Gui.InGame;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Keeps raw mouse/right-stick input available to the emote wheel without rotating the camera.
/// </summary>
public sealed partial class GameInput_GetLookDelta_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => GameInput.GetLookDelta());

    public static bool Prefix(ref Vector2 __result)
    {
        if (!EmoteWheelManager.IsOpen)
        {
            return true;
        }

        __result = Vector2.zero;
        return false;
    }
}
