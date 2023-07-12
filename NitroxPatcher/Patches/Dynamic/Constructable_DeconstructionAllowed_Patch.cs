using HarmonyLib;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Settings;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using System.Reflection;

namespace NitroxPatcher.Patches.Dynamic;

internal class Constructable_DeconstructionAllowed_Patch : NitroxPatch, IDynamicPatch
{
    internal static MethodInfo TARGET_METHOD = Reflect.Method((Constructable t) => t.DeconstructionAllowed(out Reflect.Ref<string>.Field));

    public static void Postfix(Constructable __instance, ref bool __result, ref string reason)
    {
        if (!__result || !BuildingHandler.Main || !__instance.TryGetComponentInParent(out NitroxEntity parentEntity))
        {
            return;
        }
        DeconstructionAllowed(parentEntity.Id, ref __result, ref reason);
    }

    public static void DeconstructionAllowed(NitroxId baseId, ref bool __result, ref string reason)
    {
        // TODO: Localize those strings string (same for PrefixToolConstruct)
        if (BuildingHandler.Main.BasesCooldown.ContainsKey(baseId))
        {
            __result = false;
            reason = "You can't modify a base that was recently updated by another player";
        }
        else if (BuildingHandler.Main.EnsureTracker(baseId).IsDesynced() && NitroxPrefs.SafeBuilding.Value)
        {
            __result = false;
            reason = "[Safe Building] This base is currently desynced so you can't modify it unless you resync buildings (in Nitrox settings)";
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
