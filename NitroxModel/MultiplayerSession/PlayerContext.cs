using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.MultiplayerSession;

[Serializable]
public class PlayerContext
{
    public string PlayerName { get; }
    public ushort PlayerId { get; }
    public NitroxId PlayerNitroxId { get; }
    public bool WasBrandNewPlayer { get; }
    public IntroCinematicMode IntroCinematicMode { get; set; }
    public PlayerSettings PlayerSettings { get; }
    public bool IsMuted { get; set; }

    public PlayerContext(string playerName, ushort playerId, NitroxId playerNitroxId, bool wasBrandNewPlayer, IntroCinematicMode introCinematicMode, PlayerSettings playerSettings, bool isMuted)
    {
        PlayerName = playerName;
        PlayerId = playerId;
        PlayerNitroxId = playerNitroxId;
        WasBrandNewPlayer = wasBrandNewPlayer;
        PlayerSettings = playerSettings;
        IsMuted = isMuted;
        IntroCinematicMode = introCinematicMode;
    }

    public override string ToString()
    {
        return $"[PlayerContext - PlayerName: {PlayerName}, PlayerId: {PlayerId}, PlayerNitroxId: {PlayerNitroxId}, WasBrandNewPlayer: {WasBrandNewPlayer}, PlayerSettings: {PlayerSettings}]";
    }
}
