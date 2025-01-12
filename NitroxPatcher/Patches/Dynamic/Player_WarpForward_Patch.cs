using System.Reflection;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Player_WarpForward_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Player t) => t.OnConsoleCommand_warpforward(default));

    public static bool Prefix(Player __instance, NotificationCenter.Notification n)
    {
        Vehicle currentMountedVehicle = __instance.currentMountedVehicle;
        if (!currentMountedVehicle)
        {
            return true;
        }

        // Code from the original method
        DevConsole.ParseFloat(n, 0, out var value, 3f);
        Transform aimingTransform = __instance.camRoot.GetAimingTransform();
        Vector3 targetPosition = __instance.transform.position + aimingTransform.forward * value;
        
        currentMountedVehicle.TeleportVehicle(targetPosition, currentMountedVehicle.transform.rotation);
        __instance.WaitForTeleportation();

        return false;
    }

}
