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
        // We really want to avoid unnecessary packets giving false information
        if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
        {
            return;
        }

        Resolve<LocalPlayer>().BroadcastEscapePodChange(value.GetId());
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_PROPERTY.GetSetMethod());
    }
}
