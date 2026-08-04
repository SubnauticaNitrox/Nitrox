using System.Reflection;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class MapRoomFunctionality_ObtainResourceNodes_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((MapRoomFunctionality mapRoom) => mapRoom.ObtainResourceNodes(default(TechType)));

    public static bool Prefix(MapRoomFunctionality __instance, TechType __0)
    {
        if (__instance && __instance.TryGetComponent(out ScannerRoomController controller))
        {
            return !controller.TryApplyAuthoritativeResourceNodes(__0);
        }
        return true;
    }
}
