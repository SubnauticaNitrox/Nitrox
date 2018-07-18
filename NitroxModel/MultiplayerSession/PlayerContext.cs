using System;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class PlayerContext
    {
        public string PlayerId { get; }
        public string PlayerName { get; }
        public PlayerSettings PlayerSettings { get; }

        public PlayerContext(string playerName, string playerid, PlayerSettings playerSettings)
        {
            PlayerId = playerid;
            PlayerName = playerName;
            PlayerSettings = playerSettings;
        }
    }
}
