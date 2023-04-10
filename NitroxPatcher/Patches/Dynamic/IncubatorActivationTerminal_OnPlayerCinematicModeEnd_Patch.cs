using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// When we place a power crystal into the incubator terminal it becomes consumed.  Inform the server the entity was destroyed.
/// </summary>
public class IncubatorActivationTerminal_OnPlayerCinematicModeEnd_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((IncubatorActivationTerminal t) => t.OnPlayerCinematicModeEnd(default(PlayerCinematicController)));

    public static void Prefix(IncubatorActivationTerminal __instance)
    {
        if (__instance.crystalObject)
        {
            NitroxId id = NitroxEntity.GetId(__instance.crystalObject);
            Resolve<IPacketSender>().Send(new EntityDestroyed(id));
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
