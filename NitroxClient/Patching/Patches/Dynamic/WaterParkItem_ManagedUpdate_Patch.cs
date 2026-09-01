using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

/// <summary>
/// Broadcasts the reparenting of fishes from when they move in different water parks
/// </summary>
/// <remarks>
/// Reparenting is done by <see cref="LargeRoomWaterPark.GetCell"/>
/// </remarks>
internal sealed partial class WaterParkItem_ManagedUpdate_Patch : NitroxPatch, IDynamicPatch
{
    private static SimulationOwnership simulationOwnership;
    private static IPacketSender packetSender;
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((WaterParkItem t) => t.ManagedUpdate());

    public WaterParkItem_ManagedUpdate_Patch(SimulationOwnership so, IPacketSender ps)
    {
        simulationOwnership = so;
        packetSender = ps;
    }

    public static void Prefix(WaterParkItem __instance, out WaterPark __state)
    {
        __state = __instance.currentWaterPark;
    }

    public static void Postfix(WaterParkItem __instance, WaterPark __state)
    {
        if (__state == __instance.currentWaterPark)
        {
            return;
        }

        if (__instance.TryGetIdOrWarn(out NitroxId itemId) &&
            simulationOwnership.HasAnyLockType(itemId) &&
            __instance.currentWaterPark.AliveOrNull().TryGetIdOrWarn(out NitroxId parentId))
        {
            packetSender.Send(new EntityReparented(itemId, parentId));
        }
    }
}
