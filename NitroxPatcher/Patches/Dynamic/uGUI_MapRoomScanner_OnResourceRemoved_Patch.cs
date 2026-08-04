using System.Reflection;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class uGUI_MapRoomScanner_OnResourceRemoved_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(
        (uGUI_MapRoomScanner scanner) => scanner.OnResourceRemoved(default(ResourceTrackerDatabase.ResourceInfo)));

    public static bool Prefix(uGUI_MapRoomScanner __instance) =>
        !__instance.mapRoom ||
        !__instance.mapRoom.TryGetComponent(out ScannerRoomController controller) ||
        !controller.ShouldSuppressVanillaResources;
}
