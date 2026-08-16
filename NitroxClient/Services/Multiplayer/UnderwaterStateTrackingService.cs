using Nitrox.Model.GameLogic.PlayerAnimation;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Core;

namespace NitroxClient.Services.Multiplayer;

internal sealed class UnderwaterStateTrackingService(LocalPlayer localPlayer) : IMultiplayerGameService
{
    private readonly LocalPlayer localPlayer = localPlayer;
    private AnimChangeState lastUnderwaterState = AnimChangeState.UNSET;

    public void Update()
    {
        AnimChangeState underwaterState = (AnimChangeState)(Player.main.IsUnderwaterForSwimming() ? 1 : 0);
        if (lastUnderwaterState != underwaterState)
        {
            localPlayer.AnimationChange(AnimChangeType.UNDERWATER, underwaterState);
            lastUnderwaterState = underwaterState;
        }
    }

    public void Start()
    {
    }

    public void Started()
    {
    }

    public void Stop()
    {
    }
}
