using System;
using System.Collections.Generic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.Communication.Packets.Processors;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.Serialization.World;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Vehicles;
using NitroxServer.GameLogic.Items;
using NitroxServer.GameLogic.Players;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.Communication.Packets
{
    public class PacketHandler
    {
        private readonly Dictionary<Type, PacketProcessor> authenticatedPacketProcessorsByType;
        private readonly Dictionary<Type, PacketProcessor> unauthenticatedPacketProcessorsByType;

        private readonly DefaultServerPacketProcessor defaultPacketProcessor;
        private readonly PlayerManager playerManager;

        public PacketHandler(World world)
        {
            this.playerManager = world.PlayerManager;
            defaultPacketProcessor = new DefaultServerPacketProcessor(playerManager);

            Dictionary<Type, object> ProcessorArguments = new Dictionary<Type, object>
            {
                {typeof(World), world },
                {typeof(PlayerData), world.PlayerData },
                {typeof(BaseData), world.BaseData },
                {typeof(VehicleData), world.VehicleData },
                {typeof(InventoryData), world.InventoryData },
                {typeof(GameData), world.GameData },
                {typeof(PDAStateData), world.GameData.PDAState },
                {typeof(PlayerManager), playerManager },
                {typeof(TimeKeeper), world.TimeKeeper },
                {typeof(SimulationOwnershipData), world.SimulationOwnershipData },
                {typeof(EscapePodManager), world.EscapePodManager },
                {typeof(EntityManager), new EntityManager(world.EntityData, world.BatchEntitySpawner)},
                {typeof(EntitySimulation), new EntitySimulation(world.EntityData, world.SimulationOwnershipData, world.PlayerManager) }
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
