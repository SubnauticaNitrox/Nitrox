using System.Reflection;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Player_WarpForward_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Player t) => t.OnConsoleCommand_warpforward(default));

    public static bool Prefix(Player __instance, NotificationCenter.Notification n)
    {
        float num;
        DevConsole.ParseFloat(n, 0, out num, 3f);
        Vector3 newPosition = __instance.transform.position + (__instance.camRoot.GetAimingTransform().forward * num);

        if (!__instance.currentMountedVehicle)
        {
            return true;
        }
        __instance.currentMountedVehicle.TeleportVehicle(newPosition, __instance.currentMountedVehicle.transform.rotation);
        __instance.OnPlayerPositionCheat();
        return false;
    }

}
