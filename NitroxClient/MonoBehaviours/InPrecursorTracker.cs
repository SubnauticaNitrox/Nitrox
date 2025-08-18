using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

// TODO: add this in Multiplayer
public class InPrecursorTracker : MonoBehaviour
{
    private LocalPlayer localPlayer;
    private bool lastValue;

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
        lastValue = Player.main.precursorOutOfWater;
    }

    public void Update()
    {
        bool inPrecursor = Player.main.precursorOutOfWater;
        if (inPrecursor != lastValue)
        {
            lastValue = inPrecursor;
            localPlayer.InPrecursorChange(inPrecursor);
        }
    }
}
