using System;
using System.Linq;
using System.Reflection;
using NitroxClient.MonoBehaviours.Gui.Input;

namespace NitroxPatcher.Patches.Persistent;

/// <summary>
/// Specific patch for GameInput.Button enum to return also our own values.
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
        
        GameInput.Button[] result =
        [
            .. __result.Cast<GameInput.Button>(),
            .. Enumerable.Range(KeyBindingManager.NITROX_BASE_ID, KeyBindingManager.KeyBindings.Count).Cast<GameInput.Button>()
        ];
            
        __result = result;
    }
}
