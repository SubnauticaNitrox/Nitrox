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
    /// <summary>
    /// Not null if the player is currently occupying a Seamoth as a passenger. Passenger state never grants simulation ownership or pilot authority.
    /// </summary>
    public NitroxId? PassengerSeamoth { get; set; }
    public byte SeamothPassengerSeat { get; set; }
    public IntroCinematicMode IntroCinematicMode { get; set; }
    public PlayerAnimation Animation { get; set; }

    public PlayerContext(string playerName, SessionId sessionId, NitroxId playerNitroxId, bool wasBrandNewPlayer, PlayerSettings playerSettings, bool isMuted,
                         SubnauticaGameMode gameMode, NitroxId? drivingVehicle, IntroCinematicMode introCinematicMode, PlayerAnimation animation,
                         NitroxId? passengerSeamoth = null, byte seamothPassengerSeat = 0)
    {
        PlayerName = playerName;
        SessionId = sessionId;
        PlayerNitroxId = playerNitroxId;
        WasBrandNewPlayer = wasBrandNewPlayer;
        PlayerSettings = playerSettings;
        IsMuted = isMuted;
        GameMode = gameMode;
        DrivingVehicle = drivingVehicle;
        PassengerSeamoth = passengerSeamoth;
        SeamothPassengerSeat = seamothPassengerSeat;
        IntroCinematicMode = introCinematicMode;
        Animation = animation;
    }

    public override string ToString()
    {
        return $"[{nameof(PlayerContext)} PlayerName: {PlayerName}, {nameof(SessionId)}: {SessionId}, PlayerNitroxId: {PlayerNitroxId}, WasBrandNewPlayer: {WasBrandNewPlayer}, PlayerSettings: {PlayerSettings}, GameMode: {GameMode}, DrivingVehicle: {DrivingVehicle}, PassengerSeamoth: {PassengerSeamoth}, SeamothPassengerSeat: {SeamothPassengerSeat}, IntroCinematicMode: {IntroCinematicMode}, Animation: {Animation}]";
    }
}
