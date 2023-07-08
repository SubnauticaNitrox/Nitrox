using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.Helpers;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public class Fire_Douse_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Fire t) => t.Douse(default(float)));

    public static void Postfix(Fire __instance)
    {
        NitroxId nitroxId = GetId(__instance);

        if (nitroxId == null)
        {
#if DEBUG
            Log.Error($"Fire instance is unknown to the server {__instance.gameObject.name}");
#endif
            return;
        }

        if (!__instance.livemixin.IsAlive() || __instance.IsExtinguished())
        {
            // Ensure one final metadata packet goes out to trigger any residual extinguish event that need to happen.
            // We want to make sure this packet happens before the delete, so send it directly and not with throttling.
            Resolve<ThrottledPacketSender>().RemovePendingPackets(nitroxId);
            Resolve<Entities>().EntityMetadataChanged(__instance, nitroxId);

            // Then tell the serve to delete the entity as it is now extinguished. 
            Resolve<IPacketSender>().Send(new EntityDestroyed(nitroxId));
        }
        else
        {
            Resolve<Entities>().EntityMetadataChangedThrottled(__instance, nitroxId);
        }
    }

    /// <summary>
    /// In some cases, fires are directly tagged as NitroxEntities.  In other cases, they are spawned later by
    /// Prefab placeholders and are not tagged directly.  In either case, we want to find the id on this object
    /// but only looking at the object and its direct parent (we don't want to go any higher, just in case). 
    /// </summary>
    private static NitroxId GetId(Fire fire)
    {
        NitroxEntity currentEntity = fire.GetComponent<NitroxEntity>();

        if (currentEntity)
        {
            return currentEntity.Id;
        }

        NitroxEntity parentEntity = fire.transform.parent.GetComponent<NitroxEntity>();

        if (parentEntity)
        {
            return parentEntity.Id;
        }

        return null;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
