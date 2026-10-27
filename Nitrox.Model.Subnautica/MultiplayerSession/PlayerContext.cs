using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.MultiplayerSession;

[Serializable]
public class PlayerContext(
    string playerName,
    SessionId sessionId,
    NitroxId playerNitroxId,
    bool wasBrandNewPlayer,
    PlayerSettings playerSettings,
    bool isMuted,
    SubnauticaGameMode gameMode,
    NitroxId? drivingVehicle,
    IntroCinematicMode introCinematicMode,
    PlayerAnimation animation)
{
    public string PlayerName { get; } = playerName;
    public SessionId SessionId { get; } = sessionId;

    /// <summary>
    ///     Use PeerId instead.
    /// </summary>
    public NitroxId PlayerNitroxId { get; } = playerNitroxId;

    public bool WasBrandNewPlayer { get; } = wasBrandNewPlayer;
    public PlayerSettings PlayerSettings { get; } = playerSettings;
    public bool IsMuted { get; set; } = isMuted;
    public SubnauticaGameMode GameMode { get; set; } = gameMode;

    /// <summary>
    ///     Not null if the player is currently driving a vehicle.
    /// </summary>
    public NitroxId? DrivingVehicle { get; set; } = drivingVehicle;

    public IntroCinematicMode IntroCinematicMode { get; set; } = introCinematicMode;
    public PlayerAnimation Animation { get; set; } = animation;

    public override string ToString()
    {
        return
            $"[{nameof(PlayerContext)} PlayerName: {PlayerName}, {nameof(SessionId)}: {SessionId}, PlayerNitroxId: {PlayerNitroxId}, WasBrandNewPlayer: {WasBrandNewPlayer}, PlayerSettings: {PlayerSettings}, GameMode: {GameMode}, DrivingVehicle: {DrivingVehicle}, IntroCinematicMode: {IntroCinematicMode}, Animation: {Animation}]";
    }
}
