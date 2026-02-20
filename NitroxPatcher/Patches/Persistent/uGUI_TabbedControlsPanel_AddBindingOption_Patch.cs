using System;
using System.Reflection;
using NitroxClient.MonoBehaviours.Gui.Input;

namespace NitroxPatcher.Patches.Persistent;

/// <summary>
/// Intercepts binding options created by Nitrox to use the proper label instead of "Option[ButtonID]".
/// We modify the label by reference so the original method runs once with the correct display text,
/// avoiding recursive calls to AddBindingOption which would break the uGUI_Bindings component.
/// </summary>
public partial class uGUI_TabbedControlsPanel_AddBindingOption_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_TabbedControlsPanel t) => t.AddBindingOption(default, default, default, default));

    private const string SUBNAUTICA_OPTION_PREFIX = "Option";

    public static void Prefix(ref string label, GameInput.Button button)
    {
        try
        {
            // Check if this is a Nitrox button by its ID and translate the label
            int buttonId = (int)button;
            if (buttonId >= KeyBindingManager.NITROX_BASE_ID && buttonId < KeyBindingManager.NITROX_BASE_ID + KeyBindingManager.KeyBindings.Count)
            {
                if (GameInput.ActionNames.valueToString.TryGetValue(button, out string localizationKey))
                {
                    label = Language.main != null ? Language.main.Get(localizationKey) : localizationKey;
                    return;
                }
            }

            // Fallback: check for "OptionNitrox" prefix (label set by the game as "Option" + actionName)
            if (label.StartsWith(SUBNAUTICA_OPTION_PREFIX + "Nitrox"))
            {
                string localizationKey = label[SUBNAUTICA_OPTION_PREFIX.Length..];
                label = Language.main != null ? Language.main.Get(localizationKey) : localizationKey;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in uGUI_TabbedControlsPanel_AddBindingOption_Patch");
        }
    }
}
