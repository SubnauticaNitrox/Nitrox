using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

/// <summary>
///     Broadcasts the destruction of a Flare once it's out of energy if the local player simulates it.
/// </summary>
internal sealed partial class Flare_OnDestroy_Patch : NitroxPatch, IDynamicPatch
{
    private static SimulationOwnership simulationOwnership;
    private static IPacketSender packetSender;
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Flare t) => t.OnDestroy());

    public Flare_OnDestroy_Patch(SimulationOwnership so, IPacketSender ps)
    {
        simulationOwnership = so;
        packetSender = ps;
    }

    public static void Prefix(Flare __instance)
    {
        if (__instance.TryGetNitroxId(out NitroxId flareId) &&
            simulationOwnership.HasAnyLockType(flareId))
        {
            packetSender.Send(new EntityDestroyed(flareId));
        }
    }
}
