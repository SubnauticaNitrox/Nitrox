using System;
using System.Linq;
using System.Reflection;
using NitroxClient.MonoBehaviours.Gui.Input;

namespace NitroxPatcher.Patches.Persistent;

/// <summary>
/// Extends Enum.GetValues() to include Nitrox buttons, which is required for the rebinding UI to recognize them as valid.
/// Duplicate checking ensures compatibility when Nautilus or other mods also extend the enum.
/// GameInputSystem_Initialize_Patch also extends GameInput.AllActions (with its own duplicate check) for action creation.
/// </summary>
public partial class Enum_GetValues_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => Enum.GetValues(default));

    public static void Postfix(Type enumType, ref Array __result)
    {
        if (enumType != typeof(GameInput.Button))
        {
            return;
        }

        // Check if Nitrox buttons are already in the result (e.g. added by Nautilus or a previous call)
        int firstNitroxButton = KeyBindingManager.NITROX_BASE_ID;
        if (__result.Cast<GameInput.Button>().Any(b => (int)b >= firstNitroxButton && (int)b < firstNitroxButton + KeyBindingManager.KeyBindings.Count))
        {
            return;
        }

        __result = (GameInput.Button[])
        [
            .. __result.Cast<GameInput.Button>(),
            .. Enumerable.Range(firstNitroxButton, KeyBindingManager.KeyBindings.Count).Cast<GameInput.Button>()
        ];
    }
}
