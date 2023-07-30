using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CyclopsMotorModeButton_OnClick_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsMotorModeButton t) => t.OnClick());

    public static bool Prefix(CyclopsMotorModeButton __instance, out bool __state)
    {
        SubRoot cyclops = __instance.subRoot;
        if (cyclops != null && cyclops == Player.main.currentSub)
        {
            CyclopsHelmHUDManager cyclops_HUD = cyclops.gameObject.RequireComponentInChildren<CyclopsHelmHUDManager>();
            // To show the Cyclops HUD every time "hudActive" have to be true. "hornObject" is a good indicator to check if the player piloting the cyclops.
            if (cyclops_HUD.hudActive)
            {
                __state = cyclops_HUD.hornObject.activeSelf;
                return cyclops_HUD.hornObject.activeSelf;
            }
        }

        __state = false;
        return false;
    }

    public static void Postfix(CyclopsMotorModeButton __instance, bool __state)
    {
        if (__state && __instance.subRoot.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<Cyclops>().BroadcastMetadataChange(id);
        }
    }
}
