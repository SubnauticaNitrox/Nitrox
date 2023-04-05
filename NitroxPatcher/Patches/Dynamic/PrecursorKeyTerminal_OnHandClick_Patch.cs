using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class PrecursorKeyTerminal_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorKeyTerminal t) => t.OnHandClick(default(GUIHand)));

    public static void Postfix(PrecursorKeyTerminal __instance)
    {
        if (__instance.slotted && NitroxEntity.TryGetIdOrWarn<PrecursorKeyTerminal_OnHandClick_Patch>(__instance.gameObject, out NitroxId id))
        {
            PrecursorKeyTerminalMetadata precursorKeyTerminalMetadata = new(__instance.slotted);
            Resolve<Entities>().BroadcastMetadataUpdate(id, precursorKeyTerminalMetadata);
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
