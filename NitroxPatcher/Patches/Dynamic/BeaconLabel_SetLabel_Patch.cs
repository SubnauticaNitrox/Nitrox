using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Adds a callback to broadcast beacon label change when edited.
/// </summary>
public sealed partial class BeaconLabel_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((BeaconLabel t) => t.OnHandClick(default));

    public static void Postfix(BeaconLabel __instance)
    {
        uGUI.main.userInput.callback += _ =>
        {
            if (__instance.transform.parent && __instance.transform.parent.TryGetIdOrWarn(out NitroxId id))
            {
                Resolve<Entities>().EntityMetadataChanged(__instance.transform.parent.GetComponent<Beacon>(), id);
            }
        };
    }
}
