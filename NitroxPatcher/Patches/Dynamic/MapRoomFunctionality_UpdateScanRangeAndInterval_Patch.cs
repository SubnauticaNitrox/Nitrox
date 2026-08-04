using System.Reflection;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class MapRoomFunctionality_UpdateScanRangeAndInterval_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomFunctionality mapRoom) => mapRoom.UpdateScanRangeAndInterval());

    public static void Postfix(MapRoomFunctionality __instance)
    {
        if (__instance && __instance.TryGetComponent(out ScannerRoomController controller))
        {
            controller.RequestSnapshotIfStateChanged();
        }
    }
}
