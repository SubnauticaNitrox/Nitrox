using System;
using System.Collections.Generic;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class SeamothPassengerStateChangedProcessor : IClientPacketProcessor<SeamothPassengerStateChanged>
{
    private readonly SeamothPassengers seamothPassengers;
    private readonly PlayerManager playerManager;
    private readonly Dictionary<SessionId, CachedPassengerState> latestStates = [];

    private static readonly TimeSpan UNKNOWN_STATE_TTL = TimeSpan.FromMinutes(5);

    public SeamothPassengerStateChangedProcessor(SeamothPassengers seamothPassengers, PlayerManager playerManager)
    {
        this.seamothPassengers = seamothPassengers;
        this.playerManager = playerManager;
        playerManager.OnCreate += OnPlayerCreated;
        playerManager.OnRemove += OnPlayerRemoved;
    }

    public Task Process(ClientProcessorContext context, SeamothPassengerStateChanged packet)
    {
        DateTime now = DateTime.UtcNow;
        PruneExpired(now);

        if (seamothPassengers.LocalSessionId.HasValue && packet.SessionId == seamothPassengers.LocalSessionId.Value)
        {
            seamothPassengers.ApplyState(packet);
            return Task.CompletedTask;
        }

        if (playerManager.TryFind(packet.SessionId, out RemotePlayer remotePlayer))
        {
            ApplyRemoteState(remotePlayer, packet);
            latestStates.Remove(packet.SessionId);
        }
        else
        {
            // Empty states are cached too: they must be able to clear an older passenger snapshot during initial sync.
            latestStates[packet.SessionId] = new(packet, now.Add(UNKNOWN_STATE_TTL));
        }
        return Task.CompletedTask;
    }

    private void OnPlayerCreated(SessionId sessionId, RemotePlayer remotePlayer)
    {
        PruneExpired(DateTime.UtcNow);
        if (latestStates.TryGetValue(sessionId, out CachedPassengerState cachedState))
        {
            ApplyRemoteState(remotePlayer, cachedState.Packet);
            latestStates.Remove(sessionId);
        }
    }

    private void OnPlayerRemoved(SessionId sessionId, RemotePlayer _) => latestStates.Remove(sessionId);

    private void PruneExpired(DateTime now)
    {
        List<SessionId> expiredSessionIds = [];
        foreach (KeyValuePair<SessionId, CachedPassengerState> state in latestStates)
        {
            if (state.Value.ExpiresAt <= now)
            {
                expiredSessionIds.Add(state.Key);
            }
        }

        foreach (SessionId sessionId in expiredSessionIds)
        {
            latestStates.Remove(sessionId);
        }
    }

    private static void ApplyRemoteState(RemotePlayer remotePlayer, SeamothPassengerStateChanged packet)
    {
        if (!packet.SeamothId.HasValue)
        {
            remotePlayer.SetPassengerSeamoth(null);
            return;
        }

        if (packet.SeatIndex >= SeamothPassengerAnchors.MaxPassengers)
        {
            remotePlayer.SetPassengerSeamoth(null);
            return;
        }

        if (!NitroxEntity.TryGetComponentFrom(packet.SeamothId.Value, out SeaMoth seamoth))
        {
            // The vehicle can be outside this client's currently streamed cells. Drop any old visual attachment but
            // preserve the canonical state so VehicleEntitySpawner can restore it when this Seamoth materializes.
            remotePlayer.SetPassengerSeamoth(null);
            remotePlayer.PlayerContext.DrivingVehicle = null;
            remotePlayer.PlayerContext.PassengerSeamoth = packet.SeamothId.Value;
            remotePlayer.PlayerContext.SeamothPassengerSeat = packet.SeatIndex;
            return;
        }

        remotePlayer.SetPassengerSeamoth(seamoth, packet.SeatIndex);
    }

    private sealed class CachedPassengerState(SeamothPassengerStateChanged packet, DateTime expiresAt)
    {
        public SeamothPassengerStateChanged Packet { get; } = packet;
        public DateTime ExpiresAt { get; } = expiresAt;
    }
}
