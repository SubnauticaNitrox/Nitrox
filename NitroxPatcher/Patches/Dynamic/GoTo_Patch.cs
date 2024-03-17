using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class GoTo_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((GotoConsoleCommand gotoConsoleCommand) => gotoConsoleCommand.GotoPosition(default, default));

    public static bool Prefix(Vector3 position, bool gotoImmediate = false)
    {
        Vehicle currentMountedVehicle = Player.main.currentMountedVehicle;
        if (currentMountedVehicle == null)
        {
            return true; // Normal GoTo behaviour if not in vehicle
        }
        currentMountedVehicle.TeleportVehicle(position, currentMountedVehicle.transform.rotation); // handle GoTo with a vehicle teleport, which takes player too
        return false;
    }
}
