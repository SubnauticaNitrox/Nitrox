using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public class CyclopsDestructionEvent_OnConsoleCommand_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD_RESTORE = Reflect.Method((CyclopsDestructionEvent t) => t.OnConsoleCommand_restorecyclops(default));
    private static readonly MethodInfo TARGET_METHOD_DESTROY = Reflect.Method((CyclopsDestructionEvent t) => t.OnConsoleCommand_destroycyclops(default));

    // TornacTODO: Add a raycast to only use this command on the cyclops in front of us
    public static void PrefixRestore(CyclopsDestructionEvent __instance)
    {
        NitroxId id = NitroxEntity.GetId(__instance.gameObject);
        Resolve<IPacketSender>().Send(new CyclopsRestored(id));
    }

    public static void PrefixDestroy(CyclopsDestructionEvent __instance)
    {
        NitroxId id = NitroxEntity.GetId(__instance.gameObject);
        Resolve<IPacketSender>().Send(new CyclopsDestroyed(id, true));
    }


    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD_RESTORE, nameof(PrefixRestore));
        PatchPrefix(harmony, TARGET_METHOD_DESTROY, nameof(PrefixDestroy));
    }
}
