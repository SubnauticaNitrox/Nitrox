using System.Reflection;

namespace NitroxPatcher.Patches.Persistent;

/// <summary>
/// Intercepts binding options created by Nitrox to remove the "Option" prefix given by <see cref="GameInputSystem.PopulateBindingSettings"/>
/// </summary>
public partial class uGUI_TabbedControlsPanel_AddBindingOption_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_TabbedControlsPanel t) => t.AddBindingOption(default, default, default, default));

    private const string SUBNAUTICA_OPTION_PREFIX = "Option";
    private const string DETECT_OPTION_PREFIX = "OptionNitrox";

    public static bool Prefix(uGUI_TabbedControlsPanel __instance, int tabIndex, string label, GameInput.Device device, GameInput.Button button, ref uGUI_Bindings __result)
    {
        if (label.StartsWith(DETECT_OPTION_PREFIX))
        {
            // Cut "Option" from the label
            __result = __instance.AddBindingOption(tabIndex, label[SUBNAUTICA_OPTION_PREFIX.Length..], device, button);
            return false;
        }

        return true;
    }
}
