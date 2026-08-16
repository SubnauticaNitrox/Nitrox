using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Vehicles;

namespace NitroxClient.Patching.Patches.Dynamic;

/// <summary>
///     Broadcasts the exosuit grab by Sea Dragons (if local player has remote control of them) and temporarily disables
///     exosuit's position sync while they're grabbed.
/// </summary>
internal sealed partial class SeaDragon_GrabExosuit_Patch : NitroxPatch, IDynamicPatch
{
    private static SimulationOwnership simulationOwnership;
    private static IPacketSender packetSender;
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaDragon t) => t.GrabExosuit(default));

    public SeaDragon_GrabExosuit_Patch(SimulationOwnership so, IPacketSender ps)
    {
        simulationOwnership = so;
        packetSender = ps;
    }

    public static void Prefix(SeaDragon __instance, Exosuit exosuit)
    {
        if (exosuit.TryGetComponent(out VehicleMovementReplicator vehicleMovementReplicator))
        {
            vehicleMovementReplicator.enabled = false;
        }

        if (__instance.TryGetNitroxId(out NitroxId seaDragonId) && simulationOwnership.HasAnyLockType(seaDragonId) &&
            exosuit.TryGetNitroxId(out NitroxId targetId))
        {
            packetSender.Send(new SeaDragonGrabExosuit(seaDragonId, targetId));
        }
    }
}
