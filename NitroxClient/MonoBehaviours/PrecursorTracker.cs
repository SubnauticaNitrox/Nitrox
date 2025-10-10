using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class PrecursorTracker : MonoBehaviour
{
    private LocalPlayer localPlayer;
    private bool lastInPrecursor;
    private bool lastDisplaySurfaceWater;

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
        lastInPrecursor = Player.main.precursorOutOfWater;
        lastDisplaySurfaceWater = Player.main.displaySurfaceWater;
    }

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
}
