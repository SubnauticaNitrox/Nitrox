using System;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class PlayerContext
    {
        public string PlayerId { get; }
        public string PlayerName { get; }
        public PlayerSettings PlayerSettings { get; }

        public PlayerContext(string playerName, PlayerSettings playerSettings)
        {
            PlayerId = Guid.NewGuid().ToString();
            PlayerName = playerName;
            PlayerSettings = playerSettings;
        }
    }
}
