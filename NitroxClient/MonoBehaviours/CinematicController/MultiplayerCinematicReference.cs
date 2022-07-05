using System;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Overrides;

public class MultiplayerCinematicReference : MonoBehaviour
{
    private readonly Dictionary<string, Dictionary<int, MultiplayerCinematicController>> controllerByKey = new();

    public void CallStartCinematicMode(string key, int identifier, RemotePlayer player)
    {
        if (!controllerByKey.TryGetValue(key, out Dictionary<int, MultiplayerCinematicController> controllers))
        {
            throw new KeyNotFoundException($"There was no entry for the key {key} at {gameObject.GetFullHierarchyPath()}");
        }

        if (!controllers.TryGetValue(identifier, out MultiplayerCinematicController controller))
        {
            throw new KeyNotFoundException($"There was no entry for the identifier {identifier} at {gameObject.GetFullHierarchyPath()}");
        }

        controller.CallStartCinematicMode(player);
    }

    public void CallCinematicModeEnd(string key, int identifier, RemotePlayer player)
    {
        if (!controllerByKey.TryGetValue(key, out Dictionary<int, MultiplayerCinematicController> controllers))
        {
            throw new KeyNotFoundException($"There was no entry for the key {key} at {gameObject.GetFullHierarchyPath()}");
        }

        if (!controllers.TryGetValue(identifier, out MultiplayerCinematicController controller))
        {
            throw new KeyNotFoundException($"There was no entry for the identifier {identifier} at {gameObject.GetFullHierarchyPath()}");
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
