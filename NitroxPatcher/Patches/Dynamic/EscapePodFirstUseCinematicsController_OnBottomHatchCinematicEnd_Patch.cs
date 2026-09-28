using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts escape pod metadata when the bottom hatch first-use cinematic ends.
/// This ensures other players know the hatch has been used and won't see the first-use cinematic.
/// </summary>
public sealed partial class EscapePodFirstUseCinematicsController_OnBottomHatchCinematicEnd_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((EscapePodFirstUseCinematicsController t) => t.OnBottomHatchCinematicEnd(default));

    public static void Postfix(EscapePodFirstUseCinematicsController __instance)
    {
        // Broadcast the updated metadata so other players know the hatch has been used
        if (__instance.escapePod.TryGetIdOrWarn(out NitroxId escapePodId))
        {
            EntityMetadata metadata = Resolve<EntityMetadataManager>().Extract(__instance.escapePod.gameObject).Value;
            Resolve<IPacketSender>().Send(new EntityMetadataUpdate(escapePodId, metadata));
        }
    }
}
