using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.MultiplayerSession
{
    [ZeroFormattable]
    public class PlayerContext
    {
        [Index(0)]
        public virtual string PlayerName { get; protected set; }
        [Index(1)]
        public virtual ushort PlayerId { get; protected set; }
        [Index(2)]
        public virtual NitroxId PlayerNitroxId { get; protected set; }
        [Index(3)]
        public virtual bool WasBrandNewPlayer { get; protected set; }
        [Index(4)]
        public virtual PlayerSettings PlayerSettings { get; protected set; }

        public PlayerContext() { }

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
