using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using NitroxClient.Unity.Helper;
using Nitrox.Model.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PlayerCinematicController_OnPlayerCinematicModeEnd_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((PlayerCinematicController t) => t.OnPlayerCinematicModeEnd());

    public static void Prefix(PlayerCinematicController __instance)
    {
        if (!__instance.cinematicModeActive)
        {
            return;
        }

        if (!__instance.TryGetComponentInParent(out NitroxEntity entity, true))
        {
            Log.Warn($"[{nameof(PlayerCinematicController_OnPlayerCinematicModeEnd_Patch)}] - No NitroxEntity for \"{__instance.gameObject.GetFullHierarchyPath()}\" found!");
            return;
        }

        int identifier = __instance.gameObject.GetHierarchyPath(entity.gameObject).GetHashCode();
        Log.Debug($"[CINEMATIC_DEBUG] EndCinematicMode - Controller: {__instance.gameObject.GetFullHierarchyPath()}, Entity: {entity.gameObject.GetFullHierarchyPath()}, Identifier: {identifier}, AnimName: {__instance.playerViewAnimationName}");
        Resolve<PlayerCinematics>().EndCinematicMode(Resolve<LocalPlayer>().SessionId.Value, entity.Id, identifier, __instance.playerViewAnimationName);
    }
}
