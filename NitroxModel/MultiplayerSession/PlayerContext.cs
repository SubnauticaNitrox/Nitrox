using System;
using NitroxModel.DataStructures;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class PlayerContext
    {
        public string PlayerName { get; }
        public ushort PlayerId { get; }
        public NitroxId PlayerNitroxId { get; }
        public bool WasBrandNewPlayer { get; }
        public PlayerSettings PlayerSettings { get; }

        public PlayerContext(string playerName, ushort playerId, NitroxId playerNitroxId, bool wasBrandNewPlayer, PlayerSettings playerSettings)
        {
            PlayerName = playerName;
            PlayerId = playerId;
            PlayerNitroxId = playerNitroxId;
            WasBrandNewPlayer = wasBrandNewPlayer;
            PlayerSettings = playerSettings;
        }

        public override string ToString()
        {
            return $"[PlayerContext - PlayerName: {PlayerName}, PlayerId: {PlayerId}, PlayerNitroxId: {PlayerNitroxId}, WasBrandNewPlayer: {WasBrandNewPlayer}, PlayerSettings: {PlayerSettings}]";
        }
    }
}
