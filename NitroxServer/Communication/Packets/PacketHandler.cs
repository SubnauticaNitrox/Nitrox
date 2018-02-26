using System;
using System.Collections.Generic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxModel.Tcp;
using NitroxServer.Communication.Packets.Processors;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Spawning;

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

            Dictionary<Type, object> ProcessorArguments = new Dictionary<Type, object>
            {
                {typeof(PlayerManager), playerManager },
                {typeof(TimeKeeper), timeKeeper },
                {typeof(SimulationOwnership), simulationOwnership },
                {typeof(EscapePodManager), new EscapePodManager() },
                {typeof(EntityManager), new EntityManager(new EntitySpawner(), simulationOwnership) }
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
            if (authenticatedPacketProcessorsByType.ContainsKey(packet.GetType()))
            {
                authenticatedPacketProcessorsByType[packet.GetType()].ProcessPacket(packet, player);
            }
            else
            {
                defaultPacketProcessor.ProcessPacket(packet, player);
            }
        }

        private void ProcessUnauthenticated(Packet packet, Connection connection)
        {
            if (unauthenticatedPacketProcessorsByType.ContainsKey(packet.GetType()))
            {
                unauthenticatedPacketProcessorsByType[packet.GetType()].ProcessPacket(packet, connection);
            }
            else
            {
                Log.Info("Received invalid, unauthenticated packet: " + packet);
            }
        }
    }
}
