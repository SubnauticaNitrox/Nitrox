using System.Collections.Generic;
using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Overrides;

public class MultiplayerCinematicController : MonoBehaviour
{
    private readonly Dictionary<ushort, RemotePlayerCinematicController> controllerByPlayerId = new();

    /// <summary>
    ///     MCCs with the same Animator to reset state if needed.
    /// </summary>
    private readonly List<MultiplayerCinematicController> multiplayerControllerSameAnimator = new();

    private CinematicControllerPrefab controllerPrefab;
    private PlayerCinematicController playerController;

    public void CallStartCinematicMode(RemotePlayer player)
    {
        if (!playerController.cinematicModeActive) // Check if local player is occupying the animator.
        {
            GetController(player).StartCinematicMode(player);
        }
    }

    public void CallCinematicModeEnd(RemotePlayer player)
    {
        if (!playerController.cinematicModeActive) // Check if local player is occupying the animator.
        {
            GetController(player).OnPlayerCinematicModeEnd();
        }
    }

    public void CallAllCinematicModeEnd()
    {
        foreach (RemotePlayerCinematicController remoteController in controllerByPlayerId.Values)
        {
            remoteController.EndCinematicMode(true);
        }

        foreach (MultiplayerCinematicController controller in multiplayerControllerSameAnimator)
        {
            foreach (RemotePlayerCinematicController remoteController in controller.controllerByPlayerId.Values)
            {
                remoteController.EndCinematicMode(true);
            }
        }
    }

    private RemotePlayerCinematicController GetController(RemotePlayer player)
    {
        if (controllerByPlayerId.TryGetValue(player.PlayerId, out RemotePlayerCinematicController controller))
        {
            return controller;
        }

        player.PlayerDisconnectEvent.AddHandler(gameObject, OnPlayerDisconnect);

        controller = CreateNewControllerForPlayer();
        controllerByPlayerId.Add(player.PlayerId, controller);
        return controller;
    }

    public void OnPlayerDisconnect(RemotePlayer player)
    {
        if (controllerByPlayerId.TryGetValue(player.PlayerId, out RemotePlayerCinematicController controller))
        {
            Destroy(controller);
            controllerByPlayerId.Remove(player.PlayerId);
        }
    }

    private RemotePlayerCinematicController CreateNewControllerForPlayer()
    {
        RemotePlayerCinematicController controller = gameObject.AddComponent<RemotePlayerCinematicController>();
        controllerPrefab.PopulateRemoteController(controller);

        return controller;
    }

    public void AddOtherControllers(IEnumerable<MultiplayerCinematicController> otherControllers)
    {
        foreach (MultiplayerCinematicController controller in otherControllers)
        {
            if (controller.playerController.animator == playerController.animator)
            {
                multiplayerControllerSameAnimator.Add(controller);
            }
        }
    }

    public static MultiplayerCinematicController Initialize(PlayerCinematicController playerController)
    {
        MultiplayerCinematicController mcp = playerController.gameObject.AddComponent<MultiplayerCinematicController>();
        mcp.controllerPrefab = new CinematicControllerPrefab(playerController);
        mcp.playerController = playerController;
        return mcp;
    }
}

public readonly struct CinematicControllerPrefab
{
    private readonly Transform animatedTransform;
    private readonly Transform endTransform;
    private readonly bool onlyUseEndTransformInVr;
    private readonly bool playInVr;
    private readonly string playerViewAnimationName;
    private readonly string playerViewInterpolateAnimParam;
    private readonly string animParam;
    private readonly string interpolateAnimParam;
    private readonly float interpolationTime;
    private readonly float interpolationTimeOut;
    private readonly string receiversAnimParam;
    private readonly GameObject[] animParamReceivers;
    private readonly bool interpolateDuringAnimation;
    private readonly Animator animator;

    // Currently we don't sync playerController.informGameObject but no problem could be found while testing.
    public CinematicControllerPrefab(PlayerCinematicController playerController)
    {
        animatedTransform = playerController.animatedTransform;
        endTransform = playerController.endTransform;
        onlyUseEndTransformInVr = playerController.onlyUseEndTransformInVr;
        playInVr = playerController.playInVr;
        playerViewAnimationName = playerController.playerViewAnimationName;
        playerViewInterpolateAnimParam = playerController.playerViewInterpolateAnimParam;
        animParam = playerController.animParam;
        interpolateAnimParam = playerController.interpolateAnimParam;
        interpolationTime = playerController.interpolationTime;
        interpolationTimeOut = playerController.interpolationTimeOut;
        receiversAnimParam = playerController.receiversAnimParam;
        animParamReceivers = playerController.animParamReceivers;
        interpolateDuringAnimation = playerController.interpolateDuringAnimation;
        animator = playerController.animator;
    }

    public void PopulateRemoteController(RemotePlayerCinematicController remoteController)
    {
        remoteController.animatedTransform = animatedTransform;
        remoteController.informGameObject = null;
        remoteController.endTransform = endTransform;
        remoteController.onlyUseEndTransformInVr = onlyUseEndTransformInVr;
        remoteController.playInVr = playInVr;
        remoteController.playerViewAnimationName = playerViewAnimationName;
        remoteController.playerViewInterpolateAnimParam = playerViewInterpolateAnimParam;
        remoteController.animParam = animParam;
        remoteController.interpolateAnimParam = interpolateAnimParam;
        remoteController.interpolationTime = interpolationTime;
        remoteController.interpolationTimeOut = interpolationTimeOut;
        remoteController.receiversAnimParam = receiversAnimParam;
        remoteController.animParamReceivers = animParamReceivers;
        remoteController.interpolateDuringAnimation = interpolateDuringAnimation;
        remoteController.animator = animator;
    }
}
