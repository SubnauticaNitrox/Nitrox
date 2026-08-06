using System.Collections.Generic;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

internal enum SeamothPassengerChangeStatus
{
    Changed,
    Unchanged,
    Rejected
}

internal readonly record struct SeamothPassengerChangeResult(SeamothPassengerChangeStatus Status, SeamothPassengerStateChanged State);

/// <summary>
///     Owns the transient passenger role for Seamoths. This state is deliberately separate from simulation ownership and piloting.
/// </summary>
internal sealed class SeamothPassengerService(
    EntityRegistry entityRegistry,
    SimulationOwnershipData simulationOwnershipData,
    IPacketSender packetSender) : ISessionCleaner
{
    internal const byte MAX_PASSENGERS = 3;

    private readonly Dictionary<SessionId, PassengerOccupancy> passengersBySession = [];
    private readonly Dictionary<SessionId, NitroxId> drivenSeamothsBySession = [];
    private readonly object stateLock = new();

    public SeamothPassengerChangeResult Change(Player player, Optional<NitroxId> requestedSeamothId)
    {
        lock (stateLock)
        {
            if (player.PlayerContext == null)
            {
                return new(SeamothPassengerChangeStatus.Rejected, CurrentStateUnsafe(player.SessionId, false));
            }

            if (!requestedSeamothId.HasValue)
            {
                if (RemovePassengerUnsafe(player.SessionId, out SeamothPassengerStateChanged? exitedState))
                {
                    return new(SeamothPassengerChangeStatus.Changed, exitedState);
                }
                return new(SeamothPassengerChangeStatus.Unchanged, EmptyState(player.SessionId, true));
            }

            NitroxId seamothId = requestedSeamothId.Value;
            if (passengersBySession.TryGetValue(player.SessionId, out PassengerOccupancy? currentOccupancy))
            {
                SeamothPassengerStateChanged currentState = State(currentOccupancy, currentOccupancy.SeamothId.Equals(seamothId));
                return new(
                    currentOccupancy.SeamothId.Equals(seamothId) ? SeamothPassengerChangeStatus.Unchanged : SeamothPassengerChangeStatus.Rejected,
                    currentState);
            }

            if (player.PlayerContext.DrivingVehicle != null || !IsPassengerEntryAllowed(player, seamothId))
            {
                return new(SeamothPassengerChangeStatus.Rejected, EmptyState(player.SessionId, false));
            }

            if (!TryGetAvailableSeatUnsafe(seamothId, out byte seatIndex))
            {
                return new(SeamothPassengerChangeStatus.Rejected, EmptyState(player.SessionId, false));
            }

            PassengerOccupancy occupancy = new(player, seamothId, seatIndex);
            passengersBySession.Add(player.SessionId, occupancy);
            player.PlayerContext.PassengerSeamoth = seamothId;
            player.PlayerContext.SeamothPassengerSeat = seatIndex;
            return new(SeamothPassengerChangeStatus.Changed, State(occupancy, true));
        }
    }

    /// <summary>
    ///     Keeps the passenger role mutually exclusive with driving and clears all passengers when their driver exits.
    /// </summary>
    public IReadOnlyList<SeamothPassengerStateChanged> HandlePilotModeChanged(Player player, NitroxId vehicleId, bool isPiloting)
    {
        List<SeamothPassengerStateChanged> clearedStates = [];
        lock (stateLock)
        {
            if (isPiloting)
            {
                if (RemovePassengerUnsafe(player.SessionId, out SeamothPassengerStateChanged? passengerState))
                {
                    clearedStates.Add(passengerState);
                }

                if (drivenSeamothsBySession.TryGetValue(player.SessionId, out NitroxId? previousVehicle) && !previousVehicle.Equals(vehicleId))
                {
                    ClearVehicleUnsafe(previousVehicle, clearedStates);
                }
                drivenSeamothsBySession[player.SessionId] = vehicleId;
            }
            else
            {
                bool wasKnownDriver = drivenSeamothsBySession.Remove(player.SessionId, out NitroxId? drivenVehicle) && drivenVehicle.Equals(vehicleId);
                bool contextSaysDriver = player.PlayerContext?.DrivingVehicle?.Equals(vehicleId) == true;
                if (wasKnownDriver || contextSaysDriver)
                {
                    ClearVehicleUnsafe(vehicleId, clearedStates);
                }
            }
        }
        return clearedStates;
    }

    public IReadOnlyList<SeamothPassengerStateChanged> ClearVehicle(NitroxId vehicleId)
    {
        List<SeamothPassengerStateChanged> clearedStates = [];
        lock (stateLock)
        {
            ClearVehicleUnsafe(vehicleId, clearedStates);
        }
        return clearedStates;
    }

    public IReadOnlyList<SeamothPassengerStateChanged> ClearPlayerLifecycle(Player player)
    {
        List<SeamothPassengerStateChanged> clearedStates = [];
        lock (stateLock)
        {
            if (RemovePassengerUnsafe(player.SessionId, out SeamothPassengerStateChanged? passengerState))
            {
                clearedStates.Add(passengerState);
            }

            NitroxId? drivenVehicle = null;
            if (drivenSeamothsBySession.Remove(player.SessionId, out NitroxId? trackedVehicle))
            {
                drivenVehicle = trackedVehicle;
            }
            else if (player.PlayerContext?.DrivingVehicle is NitroxId contextVehicle)
            {
                drivenVehicle = contextVehicle;
            }

            if (drivenVehicle != null)
            {
                ClearVehicleUnsafe(drivenVehicle, clearedStates);
            }
            if (player.PlayerContext != null)
            {
                player.PlayerContext.DrivingVehicle = null;
            }
        }
        return clearedStates;
    }

    public async Task OnEventAsync(ISessionCleaner.Args args)
    {
        List<SeamothPassengerStateChanged> clearedStates = [];
        lock (stateLock)
        {
            if (RemovePassengerUnsafe(args.Session.Id, out SeamothPassengerStateChanged? passengerState))
            {
                clearedStates.Add(passengerState);
            }
            if (drivenSeamothsBySession.Remove(args.Session.Id, out NitroxId? drivenVehicle))
            {
                ClearVehicleUnsafe(drivenVehicle, clearedStates);
            }
        }

        foreach (SeamothPassengerStateChanged state in clearedStates)
        {
            await packetSender.SendPacketToAllAsync(state);
        }
    }

    private bool IsPassengerEntryAllowed(Player passenger, NitroxId seamothId)
    {
        if (!entityRegistry.TryGetEntityById(seamothId, out VehicleEntity? vehicleEntity) ||
            !string.Equals(vehicleEntity.TechType.Name, "Seamoth", StringComparison.Ordinal))
        {
            return false;
        }

        return simulationOwnershipData.TryGetLock(seamothId, out SimulationOwnershipData.PlayerLock playerLock) &&
               playerLock.LockType == SimulationLockType.EXCLUSIVE &&
               playerLock.Player != passenger &&
               playerLock.Player.PlayerContext?.DrivingVehicle?.Equals(seamothId) == true;
    }

    private bool TryGetAvailableSeatUnsafe(NitroxId seamothId, out byte seatIndex)
    {
        Span<bool> occupiedSeats = stackalloc bool[MAX_PASSENGERS];
        foreach (PassengerOccupancy occupancy in passengersBySession.Values)
        {
            if (occupancy.SeamothId.Equals(seamothId) && occupancy.SeatIndex < MAX_PASSENGERS)
            {
                occupiedSeats[occupancy.SeatIndex] = true;
            }
        }

        for (byte index = 0; index < MAX_PASSENGERS; index++)
        {
            if (!occupiedSeats[index])
            {
                seatIndex = index;
                return true;
            }
        }
        seatIndex = 0;
        return false;
    }

    private void ClearVehicleUnsafe(NitroxId vehicleId, List<SeamothPassengerStateChanged> clearedStates)
    {
        List<SessionId> sessionsToClear = [];
        foreach ((SessionId sessionId, PassengerOccupancy occupancy) in passengersBySession)
        {
            if (occupancy.SeamothId.Equals(vehicleId))
            {
                sessionsToClear.Add(sessionId);
            }
        }
        foreach (SessionId sessionId in sessionsToClear)
        {
            if (RemovePassengerUnsafe(sessionId, out SeamothPassengerStateChanged? state))
            {
                clearedStates.Add(state);
            }
        }
    }

    private bool RemovePassengerUnsafe(SessionId sessionId, out SeamothPassengerStateChanged? state)
    {
        if (!passengersBySession.Remove(sessionId, out PassengerOccupancy? occupancy))
        {
            state = null;
            return false;
        }

        if (occupancy.Player.PlayerContext != null)
        {
            occupancy.Player.PlayerContext.PassengerSeamoth = null;
            occupancy.Player.PlayerContext.SeamothPassengerSeat = 0;
        }
        state = EmptyState(sessionId, true);
        return true;
    }

    private SeamothPassengerStateChanged CurrentStateUnsafe(SessionId sessionId, bool accepted) =>
        passengersBySession.TryGetValue(sessionId, out PassengerOccupancy? occupancy) ? State(occupancy, accepted) : EmptyState(sessionId, accepted);

    private static SeamothPassengerStateChanged State(PassengerOccupancy occupancy, bool accepted) =>
        new(occupancy.Player.SessionId, occupancy.SeamothId, occupancy.SeatIndex, accepted);

    private static SeamothPassengerStateChanged EmptyState(SessionId sessionId, bool accepted) => new(sessionId, Optional.Empty, 0, accepted);

    private sealed record PassengerOccupancy(Player Player, NitroxId SeamothId, byte SeatIndex);
}
