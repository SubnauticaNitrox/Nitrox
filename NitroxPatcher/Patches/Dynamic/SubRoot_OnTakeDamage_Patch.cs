using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Packets.Processors;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Hook onto <see cref="SubRoot.OnTakeDamage(DamageInfo)"/>. It'd be nice if this were the only hook needed, but both damage points and fires are created in a separate
/// class that doesn't necessarily finish running after OnTakeDamage finishes. Since that's the case, this is used only to stop phantom damage alerts that the owner didn't register
/// </summary>
public class SubRoot_OnTakeDamage_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubRoot t) => t.OnTakeDamage(default));

    public static bool Prefix(SubRoot __instance, DamageInfo damageInfo)
    {
        // This is a whitelisted type of damage from CyclopsDestroyedProcessor
        if (damageInfo.type == EntityDestroyedProcessor.DAMAGE_TYPE_RUN_ORIGINAL)
        {
            return true;
        }

        if (!__instance.TryGetNitroxId(out NitroxId id))
        {
            Log.Error($"[SubRoot_OnTakeDamage_Patch.Prefix()] Couldn't find an id on {__instance.gameObject.GetFullHierarchyPath()}");
            return true;
        }

        return Resolve<SimulationOwnership>().HasAnyLockType(id);
    }

    public static void Postfix(bool __runOriginal, SubRoot __instance, DamageInfo damageInfo)
    {
        // If we have lock on it, we'll notify the server that this cyclops must be destroyed
        if (__runOriginal && __instance.live.health <= 0f &&
            damageInfo.type != EntityDestroyedProcessor.DAMAGE_TYPE_RUN_ORIGINAL &&
            __instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<Vehicles>().BroadcastDestroyedVehicle(id);
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchMultiple(harmony, TARGET_METHOD, prefix: true, postfix: true);
    }
}
