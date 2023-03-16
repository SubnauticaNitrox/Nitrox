using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class Player_SetCurrentEscapePod_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly PropertyInfo TARGET_PROPERTY = Reflect.Property((Player p) => p.currentEscapePod);

    public static void Prefix(EscapePod value)
    {
        Resolve<LocalPlayer>().BroadcastEscapePodChange(NitroxEntity.GetOptionalIdFrom(value));
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_PROPERTY.GetSetMethod());
    }
}
