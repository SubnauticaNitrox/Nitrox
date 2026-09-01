using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Core;

namespace NitroxClient.Services.Multiplayer;

internal sealed class PrecursorTrackingService(LocalPlayer localPlayer) : IMultiplayerGameService
{
    private readonly LocalPlayer localPlayer = localPlayer;
    private bool lastInPrecursor;
    private bool lastDisplaySurfaceWater;

    public void Update()
    {
        bool inPrecursor = Player.main.precursorOutOfWater;
        if (inPrecursor != lastInPrecursor)
        {
            lastInPrecursor = inPrecursor;
            localPlayer.InPrecursorChange(inPrecursor);
        }

        bool displaySurfaceWater = Player.main.displaySurfaceWater;
        if (displaySurfaceWater != lastDisplaySurfaceWater)
        {
            lastDisplaySurfaceWater = displaySurfaceWater;
            localPlayer.DisplaySurfaceWaterChange(displaySurfaceWater);
        }
    }

    public void Start()
    {
        lastInPrecursor = Player.main.precursorOutOfWater;
        lastDisplaySurfaceWater = Player.main.displaySurfaceWater;
    }

    public void Started()
    {
    }

    public void Stop()
    {
    }
}
