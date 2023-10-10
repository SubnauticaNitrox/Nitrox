using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel.Server;

namespace NitroxModel.MultiplayerSession;

[Serializable]
public class PlayerContext
{
    public string PlayerName { get; }
    public ushort PlayerId { get; }
    public NitroxId PlayerNitroxId { get; }
    public bool WasBrandNewPlayer { get; }
    public PlayerSettings PlayerSettings { get; }
    public bool IsMuted { get; set; }
    public NitroxGameMode GameMode { get; set; }
    public SetIntroCinematicMode.IntroCinematicMode IntroCinematicMode { get; set; }

    public PlayerContext(string playerName, ushort playerId, NitroxId playerNitroxId, bool wasBrandNewPlayer, PlayerSettings playerSettings, bool isMuted,
                         NitroxGameMode gameMode, SetIntroCinematicMode.IntroCinematicMode introCinematicMode)
    {
        PlayerName = playerName;
        PlayerId = playerId;
        PlayerNitroxId = playerNitroxId;
        WasBrandNewPlayer = wasBrandNewPlayer;
        PlayerSettings = playerSettings;
        IsMuted = isMuted;
        GameMode = gameMode;
        IntroCinematicMode = introCinematicMode;
    }

    public override string ToString()
    {
        return $"[PlayerContext - PlayerName: {PlayerName}, PlayerId: {PlayerId}, PlayerNitroxId: {PlayerNitroxId}, WasBrandNewPlayer: {WasBrandNewPlayer}, PlayerSettings: {PlayerSettings}, GameMode: {GameMode}, IntroCinematicMode: {IntroCinematicMode}]";
    }
}
