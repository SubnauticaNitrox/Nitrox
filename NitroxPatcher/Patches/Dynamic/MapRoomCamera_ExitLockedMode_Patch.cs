using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class MapRoomCamera_ExitLockedMode_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomCamera t) => t.ExitLockedMode(default));

    public static void Postfix(MapRoomCamera __instance)
    {
        MapRoomCameraPlayerAnchor.End();

        if (!__instance.TryGetNitroxId(out NitroxId cameraId))
        {
            return;
        }

        EntityPositionBroadcaster.StopWatchingEntity(cameraId);
        MovementBroadcaster.UnregisterWatched(cameraId);
        Resolve<SimulationOwnership>().RequestSimulationLock(cameraId, SimulationLockType.TRANSIENT);
    }
}
