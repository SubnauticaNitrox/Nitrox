using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CyclopsLightingPanel_ToggleFloodlights_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsLightingPanel t) => t.ToggleFloodlights());

    public static bool Prefix(CyclopsLightingPanel __instance, out bool __state)
    {
        __state = __instance.floodlightsOn;
        return true;
    }

    public static void Postfix(CyclopsLightingPanel __instance, bool __state)
    {
        if (__state != __instance.floodlightsOn && __instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<Entities>().EntityMetadataChanged(__instance, id);
        }
    }
}
