using System.Reflection;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class GotoConsoleCommand_GotoPosition_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((GotoConsoleCommand t) => t.GotoPosition(default, default));

    public static bool Prefix(Vector3 position)
    {
        Vehicle currentMountedVehicle = Player.main.currentMountedVehicle;
        if (!currentMountedVehicle)
        {
            return true; // Normal GoTo behaviour if not in vehicle
        }
        currentMountedVehicle.TeleportVehicle(position, currentMountedVehicle.transform.rotation); // Handle GoTo with a vehicle teleport, which takes player too
        return false;
    }
}
