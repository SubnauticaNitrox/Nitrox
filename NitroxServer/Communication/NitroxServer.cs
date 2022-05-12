using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.Serialization;

namespace NitroxServer.Communication
{
    public abstract class NitroxServer
    {
        protected readonly int portNumber;
        protected readonly int maxConnections;
        protected readonly bool useUpnpPortForwarding;
        protected readonly bool useLANDiscovery;

        protected readonly PacketHandler packetHandler;
        protected readonly EntitySimulation entitySimulation;
        protected readonly Dictionary<int, NitroxConnection> connectionsByRemoteIdentifier = new();
        protected readonly PlayerManager playerManager;

        public NitroxServer(PacketHandler packetHandler, PlayerManager playerManager, EntitySimulation entitySimulation, ServerConfig serverConfig)
        {
            this.packetHandler = packetHandler;
            this.playerManager = playerManager;
            this.entitySimulation = entitySimulation;

            portNumber = serverConfig.ServerPort;
            maxConnections = serverConfig.MaxConnections;
            useUpnpPortForwarding = serverConfig.AutoPortForward;
            useLANDiscovery = serverConfig.LANDiscoveryEnabled;
        }

        public abstract bool Start();

        public abstract void Stop();

        protected void ClientDisconnected(NitroxConnection connection)
        {
            Player player = playerManager.GetPlayer(connection);

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

        protected void ProcessIncomingData(NitroxConnection connection, Packet packet)
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
}
