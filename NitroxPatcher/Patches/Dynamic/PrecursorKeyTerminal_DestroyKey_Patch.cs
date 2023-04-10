using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// When we place a key into the precursor terminal it becomes consumed.  Inform the server the entity was destroyed.
/// </summary>
public class PrecursorKeyTerminal_DestroyKey_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrecursorKeyTerminal t) => t.DestroyKey());

    public static void Prefix(PrecursorKeyTerminal __instance)
    {
        if (__instance.keyObject)
        {
            NitroxId id = NitroxEntity.GetId(__instance.keyObject);
            Resolve<IPacketSender>().Send(new EntityDestroyed(id));
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
