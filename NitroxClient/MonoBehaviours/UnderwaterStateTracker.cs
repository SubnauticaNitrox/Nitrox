using NitroxClient.GameLogic;
using Nitrox.Model.GameLogic.PlayerAnimation;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class UnderwaterStateTracker : MonoBehaviour
{
    private LocalPlayer localPlayer;
    private AnimChangeState lastUnderwaterState = AnimChangeState.UNSET;

    public void Awake()
    {
        localPlayer = this.Resolve<LocalPlayer>();
        enabled = false;

        Multiplayer.OnLoadingComplete += Enable;
    }

    public void OnDestroy()
    {
        Multiplayer.OnLoadingComplete -= Enable;
    }

    private void Enable()
    {
        enabled = true;
    }

    public void Update()
    {
        AnimChangeState underwaterState = (AnimChangeState)(Player.main.IsUnderwaterForSwimming() ? 1 : 0);
        if (lastUnderwaterState != underwaterState)
        {
            localPlayer.AnimationChange(AnimChangeType.UNDERWATER, underwaterState);
            lastUnderwaterState = underwaterState;
        }
    }
}
