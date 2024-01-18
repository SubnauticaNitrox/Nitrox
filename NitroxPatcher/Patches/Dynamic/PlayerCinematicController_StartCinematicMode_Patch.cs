using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PlayerCinematicController_StartCinematicMode_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((PlayerCinematicController t) => t.StartCinematicMode(default));

    public static void Prefix(PlayerCinematicController __instance)
    {
        if (__instance.cinematicModeActive)
        {
            return;
        }

        if (!__instance.TryGetComponentInParent(out NitroxEntity entity, true))
        {
            Log.Warn($"[{nameof(PlayerCinematicController_StartCinematicMode_Patch)}] - No NitroxEntity for \"{__instance.gameObject.GetFullHierarchyPath()}\" found!");
            return;
        }

        if (!__instance.TryGetComponent(out MultiplayerCinematicController multiplayerCinematicController))
        {
            Log.Error($"[{nameof(PlayerCinematicController_StartCinematicMode_Patch)}] - No MultiplayerCinematicController for \"{__instance.gameObject.GetFullHierarchyPath()}\" found!");
            return;
        }

        multiplayerCinematicController.CallAllCinematicModeEnd();

        int identifier = MultiplayerCinematicReference.GetCinematicControllerIdentifier(__instance.gameObject, entity.gameObject);
        Resolve<PlayerCinematics>().StartCinematicMode(Resolve<IMultiplayerSession>().Reservation.PlayerId, entity.Id, identifier, __instance.playerViewAnimationName);
    }
}
