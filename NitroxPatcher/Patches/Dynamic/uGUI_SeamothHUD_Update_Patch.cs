using System.Reflection;
using NitroxClient.MonoBehaviours.Gui.HUD;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class uGUI_SeamothHUD_Update_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_SeamothHUD t) => t.Update());

    public static void Postfix(uGUI_SeamothHUD __instance)
    {
        VehicleHornHudControl.EnsureAttached(__instance);
    }
}
