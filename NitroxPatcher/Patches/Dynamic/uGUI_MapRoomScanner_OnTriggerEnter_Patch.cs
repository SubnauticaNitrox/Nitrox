using System.Reflection;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class uGUI_MapRoomScanner_OnTriggerEnter_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_MapRoomScanner scanner) => scanner.OnTriggerEnter(default(Collider)));

    public static void Postfix(uGUI_MapRoomScanner __instance, Collider __0)
    {
        Player localPlayer = Player.main;
        if (!__0 || !localPlayer || __0.GetComponentInParent<Player>() != localPlayer)
        {
            return;
        }

        if (__instance.mapRoom && __instance.mapRoom.TryGetComponent(out ScannerRoomController controller))
        {
            controller.RequestImmediateSnapshot();
        }
    }
}
