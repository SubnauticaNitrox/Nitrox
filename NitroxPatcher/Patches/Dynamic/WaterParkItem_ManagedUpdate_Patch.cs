using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts the reparenting of fishes from when they move in different water parks
/// </summary>
/// <remarks>
/// Reparenting is done by <see cref="LargeRoomWaterPark.GetCell"/>
/// </remarks>
public sealed partial class WaterParkItem_ManagedUpdate_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((WaterParkItem t) => t.ManagedUpdate());

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
            Resolve<SimulationOwnership>().HasAnyLockType(itemId) &&
            __instance.currentWaterPark.AliveOrNull().TryGetIdOrWarn(out NitroxId parentId))
        {
            Resolve<IPacketSender>().Send(new EntityReparented(itemId, parentId));
        }
    }
}
