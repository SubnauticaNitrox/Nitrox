using System;
using System.Collections.Generic;
using System.Threading;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Serialization;
using Nitrox.Server.Subnautica.Models.Packets;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Communication;

public abstract class NitroxServer
{
    static NitroxServer()
    {
        Packet.InitSerializer();
    }

    protected readonly int portNumber;
    protected readonly int maxConnections;
    protected readonly bool useUpnpPortForwarding;
    protected readonly bool useLANBroadcast;

    protected readonly PacketHandler packetHandler;
    protected readonly EntitySimulation entitySimulation;
    protected readonly Dictionary<int, INitroxConnection> connectionsByRemoteIdentifier = new();
    protected internal readonly PlayerManager playerManager;

    public NitroxServer(PacketHandler packetHandler, PlayerManager playerManager, EntitySimulation entitySimulation, SubnauticaServerConfig serverConfig)
    {
        this.packetHandler = packetHandler;
        this.playerManager = playerManager;
        this.entitySimulation = entitySimulation;

        portNumber = serverConfig.ServerPort;
        maxConnections = serverConfig.MaxConnections;
        useUpnpPortForwarding = serverConfig.AutoPortForward;
        useLANBroadcast = serverConfig.LANDiscoveryEnabled;
    }

    public abstract bool Start(CancellationToken ct = default);

    public abstract void Stop();

    protected void ClientDisconnected(INitroxConnection connection)
    {
        global::Nitrox.Server.Subnautica.Player player = playerManager.GetPlayer(connection);

        if (player != null)
        {
            playerManager.PlayerDisconnected(connection);

            Disconnect disconnect = new(player.Id);
            playerManager.SendPacketToAllPlayers(disconnect);

            List<SimulatedEntity> ownershipChanges = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(player);

            if (ownershipChanges.Count > 0)
            {
                SimulationOwnershipChange ownershipChange = new(ownershipChanges);
                playerManager.SendPacketToAllPlayers(ownershipChange);
            }
        }
        else
        {
            playerManager.NonPlayerDisconnected(connection);
        }
    }

    protected void ProcessIncomingData(INitroxConnection connection, Packet packet)
    {
        try
        {
            packetHandler.Process(packet, connection);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Exception while processing packet: {packet}");
        }
    }
}
