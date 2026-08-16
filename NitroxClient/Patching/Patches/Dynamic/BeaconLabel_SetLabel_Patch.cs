using System;
using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

/// <summary>
/// Adds a callback to broadcast beacon label change when edited.
/// </summary>
internal sealed partial class BeaconLabel_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    private static Entities entities;
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((BeaconLabel t) => t.OnHandClick(default));

    public BeaconLabel_OnHandClick_Patch(Entities e)
    {
        entities = e ?? throw new ArgumentNullException(nameof(e));
    }

    public static void Postfix(BeaconLabel __instance)
    {
        uGUI.main.userInput.callback += _ =>
        {
            if (__instance.transform.parent && __instance.transform.parent.TryGetIdOrWarn(out NitroxId id))
            {
                entities.EntityMetadataChanged(__instance.transform.parent.GetComponent<Beacon>(), id);
            }
        };
    }
}
