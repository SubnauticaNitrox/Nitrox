using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;
using System.Reflection;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class BreakableResource_BreakIntoResources_Patch : NitroxPatch, IDynamicPatch
{
    private static MethodInfo TARGET_METHOD = Reflect.Method((BreakableResource t) => t.BreakIntoResources());

    public static void Prefix(BreakableResource __instance)
    {
        if (!__instance.TryGetNitroxEntity(out NitroxEntity destroyedEntity))
        {
            Log.Warn($"[{nameof(BreakableResource_BreakIntoResources_Patch)}] Could not find {nameof(NitroxEntity)} for breakable entity {__instance.gameObject.GetFullHierarchyPath()}.");
            return;
        }
        // Send packet to destroy the entity
        Resolve<IPacketSender>().Send(new EntityDestroyed(destroyedEntity.Id));
    }
}
