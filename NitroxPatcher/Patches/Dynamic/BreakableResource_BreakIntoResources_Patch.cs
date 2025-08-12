using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class BreakableResource_BreakIntoResources_Patch : NitroxPatch, IDynamicPatch
{
    private static MethodInfo TARGET_METHOD = Reflect.Method((BreakableResource t) => t.BreakIntoResources());

    public static void Prefix(BreakableResource __instance)
    {
        if (!__instance.TryGetNitroxId(out NitroxId destroyedId))
        {
            Log.Warn($"[{nameof(BreakableResource_BreakIntoResources_Patch)}] Could not find {nameof(NitroxEntity)} for breakable entity {__instance.gameObject.GetFullHierarchyPath()}.");
            return;
        }

        // Case by case handling

        // Sea Treaders spawn resource chunks but we don't register them on server-side as they're auto destroyed after 60s
        // So we need to broadcast their deletion differently
        if (__instance.GetComponent<SinkingGroundChunk>())
        {
            Resolve<IPacketSender>().Send(new SeaTreaderChunkPickedUp(destroyedId));
        }
        // Generic case
        else
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(destroyedId));
        }
    }
}
