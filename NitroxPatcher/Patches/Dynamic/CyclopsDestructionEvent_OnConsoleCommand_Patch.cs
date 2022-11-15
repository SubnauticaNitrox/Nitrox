using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel_Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public class CyclopsDestructionEvent_OnConsoleCommand_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD_RESTORE = Reflect.Method((CyclopsDestructionEvent t) => t.OnConsoleCommand_restorecyclops(default));
    private static readonly MethodInfo TARGET_METHOD_DESTROY = Reflect.Method((CyclopsDestructionEvent t) => t.OnConsoleCommand_destroycyclops(default));

    public static bool PrefixRestore()
    {
        ErrorMessage.AddWarning(Language.main.Get("Nitrox_CommandNotAvailable"));
        return false;
    }

    public static bool PrefixDestroy(CyclopsDestructionEvent __instance)
    {
        // We only apply the destroy to the current Cyclops
        if (!Player.main.currentSub || Player.main.currentSub.gameObject != __instance.gameObject)
        {
            return false;
        }

        NitroxId id = NitroxEntity.GetId(__instance.gameObject);
        Resolve<IPacketSender>().Send(new CyclopsDestroyed(id, true));
        return true;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD_RESTORE, nameof(PrefixRestore));
        PatchPrefix(harmony, TARGET_METHOD_DESTROY, nameof(PrefixDestroy));
    }
}
