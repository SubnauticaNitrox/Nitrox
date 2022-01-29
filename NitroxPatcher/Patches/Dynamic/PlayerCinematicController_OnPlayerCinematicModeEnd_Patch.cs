using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class PlayerCinematicController_OnPlayerCinematicModeEnd_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((PlayerCinematicController t) => t.OnPlayerCinematicModeEnd());

    private static ushort playerId;

    public static void Prefix(PlayerCinematicController __instance)
    {
        if (!__instance.cinematicModeActive)
        {
            return;
        }

        if (!__instance.TryGetComponent(out NitroxEntity entity))
        {
            entity = __instance.GetComponentInParent<NitroxEntity>();
            if (!entity)
            {
                Log.Warn($"[{nameof(PlayerCinematicController_OnPlayerCinematicModeEnd_Patch)}] - No NitroxEntity for \"{__instance.GetFullHierarchyPath()}\" found!");
                return;
            }
        }

        int identifier = MultiplayerCinematicReference.GetCinematicControllerIdentifier(__instance.gameObject, entity.gameObject);
        Resolve<PlayerCinematics>().EndCinematicMode(playerId, entity.Id, identifier, __instance.playerViewAnimationName);
    }

    public override void Patch(Harmony harmony)
    {
        playerId = Resolve<IMultiplayerSession>().Reservation.PlayerId;
        PatchPrefix(harmony, targetMethod);
    }
}
