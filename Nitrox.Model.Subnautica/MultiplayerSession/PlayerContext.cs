using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.MultiplayerSession;

[Serializable]
public class PlayerContext
{
    public string PlayerName { get; }
    public SessionId SessionId { get; }
    public NitroxId PlayerNitroxId { get; }
    public bool WasBrandNewPlayer { get; }
    public PlayerSettings PlayerSettings { get; }
    public bool IsMuted { get; set; }
    public SubnauticaGameMode GameMode { get; set; }
    /// <summary>
    /// Not null if the player is currently driving a vehicle.
    /// </summary>
    public NitroxId? DrivingVehicle { get; set; }
    public IntroCinematicMode IntroCinematicMode { get; set; }
    public PlayerAnimation Animation { get; set; }

    public PlayerContext(string playerName, SessionId sessionId, NitroxId playerNitroxId, bool wasBrandNewPlayer, PlayerSettings playerSettings, bool isMuted,
                         SubnauticaGameMode gameMode, NitroxId? drivingVehicle, IntroCinematicMode introCinematicMode, PlayerAnimation animation)
    {
        PlayerName = playerName;
        SessionId = sessionId;
        PlayerNitroxId = playerNitroxId;
        WasBrandNewPlayer = wasBrandNewPlayer;
        PlayerSettings = playerSettings;
        IsMuted = isMuted;
        GameMode = gameMode;
        DrivingVehicle = drivingVehicle;
        IntroCinematicMode = introCinematicMode;
        Animation = animation;
    }

    public override string ToString()
    {
        return $"[{nameof(PlayerContext)} PlayerName: {PlayerName}, PlayerId: {SessionId}, PlayerNitroxId: {PlayerNitroxId}, WasBrandNewPlayer: {WasBrandNewPlayer}, PlayerSettings: {PlayerSettings}, GameMode: {GameMode}, DrivingVehicle: {DrivingVehicle}, IntroCinematicMode: {IntroCinematicMode}, Animation: {Animation}]";
    }
}
