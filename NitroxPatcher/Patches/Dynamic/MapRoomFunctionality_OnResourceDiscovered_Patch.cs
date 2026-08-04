using System.Reflection;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class MapRoomFunctionality_OnResourceDiscovered_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(
        (MapRoomFunctionality mapRoom) => mapRoom.OnResourceDiscovered(default(ResourceTrackerDatabase.ResourceInfo)));

    public static bool Prefix(MapRoomFunctionality __instance) =>
        !__instance || !__instance.TryGetComponent(out ScannerRoomController controller) || !controller.HasAuthoritativeSnapshot;
}
