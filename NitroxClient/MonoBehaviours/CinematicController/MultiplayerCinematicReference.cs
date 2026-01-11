using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.CinematicController;

public class MultiplayerCinematicReference : MonoBehaviour
{
    private readonly Dictionary<string, Dictionary<int, MultiplayerCinematicController>> controllerByKey = new();

    private bool isEscapePod;

    private void Start()
    {
        // TODO: Currently only single EscapePod is supported, therefor EscapePod.main. Can probably be removed after we use one pod per intro sequence
        isEscapePod = gameObject == EscapePod.main.gameObject;
    }

    public void CallStartCinematicMode(string key, int identifier, RemotePlayer player)
    {
        if(isEscapePod && this.Resolve<LocalPlayer>().IntroCinematicMode is IntroCinematicMode.PLAYING or IntroCinematicMode.SINGLEPLAYER) return;

        if (!controllerByKey.TryGetValue(key, out Dictionary<int, MultiplayerCinematicController> controllers))
        {
            Log.Warn($"[{nameof(MultiplayerCinematicReference)}] No entry for key '{key}' at {gameObject.GetFullHierarchyPath()}");
            return;
        }

        if (!controllers.TryGetValue(identifier, out MultiplayerCinematicController controller))
        {
            Log.Warn($"[{nameof(MultiplayerCinematicReference)}] No entry for identifier {identifier} with key '{key}' at {gameObject.GetFullHierarchyPath()}");
            return;
        }

        controller.CallStartCinematicMode(player);
    }

    public void CallCinematicModeEnd(string key, int identifier, RemotePlayer player)
    {
        if(isEscapePod && this.Resolve<LocalPlayer>().IntroCinematicMode is IntroCinematicMode.PLAYING or IntroCinematicMode.SINGLEPLAYER) return;

        if (!controllerByKey.TryGetValue(key, out Dictionary<int, MultiplayerCinematicController> controllers))
        {
            Log.Warn($"[{nameof(MultiplayerCinematicReference)}] No entry for key '{key}' at {gameObject.GetFullHierarchyPath()}");
            return;
        }

        if (!controllers.TryGetValue(identifier, out MultiplayerCinematicController controller))
        {
            Log.Warn($"[{nameof(MultiplayerCinematicReference)}] No entry for identifier {identifier} with key '{key}' at {gameObject.GetFullHierarchyPath()}");
            return;
        }

        controller.CallCinematicModeEnd(player);
    }

    public static int GetCinematicControllerIdentifier(GameObject controller, GameObject reference) => controller.gameObject.GetHierarchyPath(reference).GetHashCode();

    public void AddController(PlayerCinematicController playerController)
    {
        MultiplayerCinematicController[] allControllers = controllerByKey.SelectMany(n => n.Value.Select(x => x.Value)).ToArray();

        if (!controllerByKey.TryGetValue(playerController.playerViewAnimationName, out Dictionary<int, MultiplayerCinematicController> controllers))
        {
            controllers = new Dictionary<int, MultiplayerCinematicController>();
            controllerByKey.Add(playerController.playerViewAnimationName, controllers);
        }

        int identifier = GetCinematicControllerIdentifier(playerController.gameObject, gameObject);

        if (controllers.ContainsKey(identifier))
        {
            return;
        }

        MultiplayerCinematicController controller = MultiplayerCinematicController.Initialize(playerController);
        controller.AddOtherControllers(allControllers);
        allControllers.ForEach(x => x.AddOtherControllers(new[] { controller }));

        controllers.Add(identifier, controller);
    }
}
