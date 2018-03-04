﻿using System;
using System.Collections.Generic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxModel.Tcp;
using NitroxServer.Communication.Packets.Processors;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Serialization;

namespace NitroxServer.Communication.Packets
{
    public class PacketHandler
    {
        private readonly Dictionary<Type, PacketProcessor> authenticatedPacketProcessorsByType;
        private readonly Dictionary<Type, PacketProcessor> unauthenticatedPacketProcessorsByType;

        private readonly DefaultServerPacketProcessor defaultPacketProcessor;
        private readonly PlayerManager playerManager;

        public PacketHandler(PlayerManager playerManager, TimeKeeper timeKeeper, SimulationOwnership simulationOwnership)
        {
            this.playerManager = playerManager;
            defaultPacketProcessor = new DefaultServerPacketProcessor(playerManager);
            ResourceAssets resourceAssets = ResourceAssetsParser.Parse();
            EntityData entityData = new EntityData();

            Dictionary<Type, object> ProcessorArguments = new Dictionary<Type, object>
            {
                {typeof(PlayerManager), playerManager },
                {typeof(TimeKeeper), timeKeeper },
                {typeof(SimulationOwnership), simulationOwnership },
                {typeof(EscapePodManager), new EscapePodManager() },
                {typeof(EntityManager), new EntityManager(entityData, new BatchEntitySpawner(resourceAssets)) },
                {typeof(EntitySimulation), new EntitySimulation(entityData, simulationOwnership) }
            };

            authenticatedPacketProcessorsByType = PacketProcessor.GetProcessors(ProcessorArguments, p => p.BaseType.IsGenericType && p.BaseType.GetGenericTypeDefinition() == typeof(AuthenticatedPacketProcessor<>));

            unauthenticatedPacketProcessorsByType = PacketProcessor.GetProcessors(ProcessorArguments, p => p.BaseType.IsGenericType && p.BaseType.GetGenericTypeDefinition() == typeof(UnauthenticatedPacketProcessor<>));
        }

        public void Process(Packet packet, Connection connection)
        {
            Player player = playerManager.GetPlayer(connection);

            if (player == null)
            {
                ProcessUnauthenticated(packet, connection);
            }
            else
            {
                ProcessAuthenticated(packet, player);
            }
        }

        private void ProcessAuthenticated(Packet packet, Player player)
        {
            PacketProcessor packetProcessor;
            if (authenticatedPacketProcessorsByType.TryGetValue(packet.GetType(), out packetProcessor))
            {
                packetProcessor.ProcessPacket(packet, player);
            }
            else
            {
                defaultPacketProcessor.ProcessPacket(packet, player);
            }
        }

        private void ProcessUnauthenticated(Packet packet, Connection connection)
        {
            PacketProcessor packetProcessor;
            if (unauthenticatedPacketProcessorsByType.TryGetValue(packet.GetType(), out packetProcessor))
            {
                packetProcessor.ProcessPacket(packet, connection);
            }
            else
            {
                Log.Info("Received invalid, unauthenticated packet: " + packet);
            }
        }
    }
}
