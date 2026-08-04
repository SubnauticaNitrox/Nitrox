using System.Reflection;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class uGUI_MapRoomScanner_UpdateAvailableTechTypes_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_MapRoomScanner scanner) => scanner.UpdateAvailableTechTypes());

    public static bool Prefix(uGUI_MapRoomScanner __instance)
    {
        if (__instance.mapRoom && __instance.mapRoom.TryGetComponent(out ScannerRoomController controller))
        {
            return !controller.TryApplyAuthoritativeResourceList(__instance);
        }
        return true;
    }
}
