using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Server;
using NitroxServer.Communication.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.NetworkingLayer
{
    public abstract class NitroxServer
    {
        protected bool isStopped = true;
        protected int portNumber, maxConn;

        protected readonly PacketHandler packetHandler;
        protected readonly EntitySimulation entitySimulation;
        protected readonly Dictionary<long, NitroxConnection> connectionsByRemoteIdentifier = new Dictionary<long, NitroxConnection>();
        protected readonly PlayerManager playerManager;
        protected readonly EntityManager entityManager;

        protected readonly Task playerBackgroundThread;

        public NitroxServer(PacketHandler packetHandler, PlayerManager playerManager, EntitySimulation entitySimulation, ServerConfig serverConfig, EntityManager entityManager)
        {
            this.packetHandler = packetHandler;
            this.playerManager = playerManager;
            this.entitySimulation = entitySimulation;
            this.entityManager = entityManager;

            portNumber = serverConfig.ServerPort;
            maxConn = serverConfig.MaxConnections;


            playerBackgroundThread = new Task(PlayerBackgroundThread, TaskCreationOptions.LongRunning);
            playerBackgroundThread.Start();
        }

        private void PlayerBackgroundThread()
        {
            while (true)
            {
                if (playerManager.AnyConnectedPlayers)
                {
                    foreach (Player player in playerManager.GetConnectedPlayers())
                    {
                        CellChanges cellChanges = player.ProcessCellChanges();

                        SendNewlyVisibleEntities(player, cellChanges);

                        Dictionary<NitroxInt3, List<SimulatedEntity>> ownershipChanges = entitySimulation.CalculateSimulationChangesFromCellSwitch(player, cellChanges);
                        BroadcastSimulationChanges(ownershipChanges);
                    }
                }

                Thread.Sleep(20);
            }
        }

        private void SendNewlyVisibleEntities(Player player, CellChanges cellChanges)
        {
            Dictionary<NitroxInt3, List<Entity>> newlyVisibleEntities = entityManager.GetVisibleEntities(cellChanges);

            if (newlyVisibleEntities.Count > 0)
            {
                foreach (NitroxInt3 batchId in newlyVisibleEntities.Keys)
                {
                    CellEntities cellEntities = new CellEntities(newlyVisibleEntities[batchId]);
                    player.SendPacket(cellEntities);
                }
            }
        }

        private void BroadcastSimulationChanges(Dictionary<NitroxInt3, List<SimulatedEntity>> ownershipChanges)
        {
            if (ownershipChanges.Count > 0)
            {
                foreach (NitroxInt3 batchId in ownershipChanges.Keys)
                {
                    // TODO: This should be moved to `SimulationOwnership`
                    SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(ownershipChanges[batchId]);
                    playerManager.SendPacketToAllPlayers(ownershipChange);
                }
            }
        }

        public abstract bool Start();

        public abstract void Stop();
        
        protected void ClientDisconnected(NitroxConnection connection)
        {
            Player player = playerManager.GetPlayer(connection);

            if (player != null)
            {
                playerManager.PlayerDisconnected(connection);

                Disconnect disconnect = new Disconnect(player.Id);
                playerManager.SendPacketToAllPlayers(disconnect);

                Dictionary<NitroxInt3, List<SimulatedEntity>> ownershipChanges = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(player);

                if (ownershipChanges.Count > 0)
                {
                    foreach (NitroxInt3 batchId in ownershipChanges.Keys)
                    {
                        SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(ownershipChanges[batchId]);
                        playerManager.SendPacketToAllPlayers(ownershipChange);
                    }
                }
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
                Log.Error("Exception while processing packet: " + packet + " " + ex);
            }
        }
    }
}
