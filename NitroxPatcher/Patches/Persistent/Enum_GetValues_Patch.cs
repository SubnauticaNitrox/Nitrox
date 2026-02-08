using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Persistent;

/// <summary>
/// Patch for GameInput.Button enum. Nitrox buttons are not added here to avoid duplicate keybinds in the input settings:
/// the game builds the binding list from both Enum.GetValues(GameInput.Button) and GameInput.AllActions when they differ.
/// We extend AllActions in GameInputSystem_Initialize_Patch instead, so the binding UI gets Nitrox keys from that single source.
/// </summary>
public partial class Enum_GetValues_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => Enum.GetValues(default));

    public static void Postfix(Type enumType, ref Array __result)
    {
        // Intentionally do not extend __result with Nitrox buttons. They are added via GameInput.AllActions
        // in GameInputSystem_Initialize_Patch so the settings UI shows each keybind once.
        if (enumType != typeof(GameInput.Button))
        {
            return;
        }
        // Leave __result unchanged (original enum values only).
    }
}
