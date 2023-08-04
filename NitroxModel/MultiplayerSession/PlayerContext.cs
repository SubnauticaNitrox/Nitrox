using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
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
    /// <summary>
    /// Not null if the player is currently driving a vehicle.
    /// </summary>
    public NitroxId DrivingVehicle { get; set; }
    public IntroCinematicMode IntroCinematicMode { get; set; }

    public PlayerContext(string playerName, ushort playerId, NitroxId playerNitroxId, bool wasBrandNewPlayer, PlayerSettings playerSettings, bool isMuted,
                         NitroxGameMode gameMode, NitroxId drivingVehicle, IntroCinematicMode introCinematicMode)
    {
        PlayerName = playerName;
        PlayerId = playerId;
        PlayerNitroxId = playerNitroxId;
        WasBrandNewPlayer = wasBrandNewPlayer;
        PlayerSettings = playerSettings;
        IsMuted = isMuted;
        GameMode = gameMode;
        DrivingVehicle = drivingVehicle;
        IntroCinematicMode = introCinematicMode;
    }

    public override string ToString()
    {
        return $"[PlayerContext - PlayerName: {PlayerName}, PlayerId: {PlayerId}, PlayerNitroxId: {PlayerNitroxId}, WasBrandNewPlayer: {WasBrandNewPlayer}, PlayerSettings: {PlayerSettings}, GameMode: {GameMode}, DrivingVehicle: {DrivingVehicle}, IntroCinematicMode: {IntroCinematicMode}]";
    }
}
