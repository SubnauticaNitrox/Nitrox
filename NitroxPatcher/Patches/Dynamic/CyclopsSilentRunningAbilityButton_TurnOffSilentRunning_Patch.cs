using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CyclopsSilentRunningAbilityButton_TurnOffSilentRunning_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsSilentRunningAbilityButton t) => t.TurnOffSilentRunning());

    public static void Postfix(CyclopsSilentRunningAbilityButton __instance)
    {
        if (__instance.subRoot.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<Cyclops>().BroadcastMetadataChange(id);
        }
    }
}
