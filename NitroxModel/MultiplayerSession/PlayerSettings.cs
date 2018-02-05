using UnityEngine;

namespace NitroxModel.MultiplayerSession
{
    public class PlayerSettings
    {
        public Color PlayerColor { get; }

        public PlayerSettings()
        {
            //This is done intentionally. This is only a stub for future extension.
            PlayerColor = new Color(255, 0, 0);
        }
    }
}
