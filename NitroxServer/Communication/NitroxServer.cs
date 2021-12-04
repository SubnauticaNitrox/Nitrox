using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.NetworkingLayer;
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

        protected readonly PacketHandler packetHandler;
        protected readonly EntitySimulation entitySimulation;
        protected readonly Dictionary<long, INitroxConnection> connectionsByRemoteIdentifier = new();
        protected readonly PlayerManager playerManager;

        public NitroxServer(PacketHandler packetHandler, PlayerManager playerManager, EntitySimulation entitySimulation, ServerConfig serverConfig)
        {
            this.packetHandler = packetHandler;
            this.playerManager = playerManager;
            this.entitySimulation = entitySimulation;

            portNumber = serverConfig.ServerPort;
            maxConnections = serverConfig.MaxConnections;
            useUpnpPortForwarding = serverConfig.AutoPortForward;
        }

        public abstract bool Start();

        public abstract void Stop();
        
        protected void ClientDisconnected(INitroxConnection connection)
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
        }

        protected void BeginPortForward(int port)
        {
            PortForward.TryOpenPortAsync(port, TimeSpan.FromSeconds(20)).ContinueWith(task =>
            {
                if (task.Result)
                {
                    Log.Info($"Server port {port} UDP has been automatically opened on your router (expires in 1 day)");
                }
                else
                {
                    Log.Warn(PortForward.GetError(port) ?? $"Failed to automatically port forward {port} UDP through UPnP. If using Hamachi or manually port-forwarding, please disregard this warning. To disable this feature you can go into the server settings.");
                }
            }).ConfigureAwait(false);
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
}
