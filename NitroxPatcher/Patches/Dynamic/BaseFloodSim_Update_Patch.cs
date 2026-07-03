using System.Reflection;
using NitroxClient.GameLogic;
using Nitrox.Model.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Periodically (throttled) broadcasts a base's current flood/water-level state, so that it can be restored for
/// players who (re)join later instead of them seeing the base's interior flooding restart from empty.
/// </summary>
public sealed partial class BaseFloodSim_Update_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((BaseFloodSim t) => t.Update());

    public static void Postfix(BaseFloodSim __instance)
    {
        if (!__instance.IsInitialized() || !__instance.baseComp || !__instance.baseComp.TryGetIdOrWarn(out NitroxId id))
        {
            return;
        }

        Resolve<Entities>().EntityMetadataChangedThrottled(__instance, id, 2f);
    }
}
