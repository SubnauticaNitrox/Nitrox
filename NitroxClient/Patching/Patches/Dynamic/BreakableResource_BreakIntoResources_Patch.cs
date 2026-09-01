using System;
using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.Patching.Patches.Dynamic;

public sealed partial class BreakableResource_BreakIntoResources_Patch : NitroxPatch, IDynamicPatch
{
    private static IPacketSender packetSender;
    private static MethodInfo TARGET_METHOD = Reflect.Method((BreakableResource t) => t.BreakIntoResources());

    public BreakableResource_BreakIntoResources_Patch(IPacketSender ps)
    {
        packetSender = ps ?? throw new ArgumentNullException(nameof(ps));
    }

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
            packetSender.Send(new SeaTreaderChunkPickedUp(destroyedId));
        }
        // Generic case
        else
        {
            packetSender.Send(new EntityDestroyed(destroyedId));
        }
    }
}
