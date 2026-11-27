using System;
using System.Collections.Generic;
using Nitrox.Model.GameLogic.PlayerAnimation;
using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class PlayerAnimatorStateBroadcaster : MonoBehaviour
{
    private LocalPlayer localPlayer;
    private readonly Dictionary<string, bool> lastAnimatorParameters = new(StringComparer.Ordinal);
    private bool sentInitialState;

    public void Awake()
    {
        localPlayer = this.Resolve<LocalPlayer>();
        Player.main.playerRespawnEvent.AddHandler(this, OnPlayerRespawn);
    }

    public void Update()
    {
        if (!Multiplayer.Main.InitialSyncCompleted || !Player.main || !Player.main.IsAlive())
        {
            return;
        }

        Dictionary<string, bool> changedParameters = [];
        Animator animator = localPlayer.PlayerModel.GetComponent<Animator>();
        foreach (string parameterName in PlayerAnimatorParameter.SynchronizedParameters)
        {
            bool value = animator.GetBool(parameterName);
            if (!sentInitialState || !lastAnimatorParameters.TryGetValue(parameterName, out bool lastValue) || lastValue != value)
            {
                changedParameters[parameterName] = value;
                lastAnimatorParameters[parameterName] = value;
            }
        }

        if (changedParameters.Count > 0)
        {
            localPlayer.BroadcastAnimatorParameters(changedParameters);
            sentInitialState = true;
        }
    }

    public void OnDestroy()
    {
        if (Player.main)
        {
            Player.main.playerRespawnEvent.RemoveHandler(this, OnPlayerRespawn);
        }
    }

    private void OnPlayerRespawn(Player player)
    {
        lastAnimatorParameters.Clear();
        sentInitialState = false;
    }
}
