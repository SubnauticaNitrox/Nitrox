using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class VehicleDockingBay_OnUndockingComplete_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((VehicleDockingBay t) => t.OnUndockingComplete(default(Player)));

    public static void Prefix(VehicleDockingBay __instance)
    {
        if (!__instance.TryGetIdOrWarn(out NitroxId dockId) ||
            !__instance.GetDockedVehicle().TryGetIdOrWarn(out NitroxId vehicleId))
        {
            return;
        }

        Resolve<IPacketSender>().Send(new VehicleUndocking(vehicleId, dockId, Resolve<IMultiplayerSession>().Reservation.PlayerId, false));
    }
}
