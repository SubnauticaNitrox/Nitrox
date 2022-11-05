using System;
using System.Threading;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.Communication.Packets.Processors;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets
{
    public class PacketHandler
    {
        private readonly ThreadLocal<Type[]> makeGenericParamsSharedArray = new(() => new Type[1]);
        private readonly PlayerManager playerManager;
        private readonly DefaultServerPacketProcessor defaultServerPacketProcessor;

        public PacketHandler(PlayerManager playerManager, DefaultServerPacketProcessor packetProcessor)
        {
            this.playerManager = playerManager;
            defaultServerPacketProcessor = packetProcessor;
        }

        public void Process(Packet packet, NitroxConnection connection)
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
            Type serverPacketProcessorType = typeof(AuthenticatedPacketProcessor<>);
            makeGenericParamsSharedArray.Value[0] = packet.GetType();
            Type packetProcessorType = serverPacketProcessorType.MakeGenericType(makeGenericParamsSharedArray.Value);

            Optional<object> opProcessor = NitroxServiceLocator.LocateOptionalService(packetProcessorType);

            if (opProcessor.HasValue)
            {
                PacketProcessor processor = (PacketProcessor)opProcessor.Value;
                processor.ProcessPacket(packet, player);
            }
            else
            {
                defaultServerPacketProcessor.ProcessPacket(packet, player);
            }
        }

        private void ProcessUnauthenticated(Packet packet, NitroxConnection connection)
        {
            try
            {
                Type serverPacketProcessorType = typeof(UnauthenticatedPacketProcessor<>);
                makeGenericParamsSharedArray.Value[0] = packet.GetType();
                Type packetProcessorType = serverPacketProcessorType.MakeGenericType(makeGenericParamsSharedArray.Value);

                PacketProcessor processor = (PacketProcessor)NitroxServiceLocator.LocateService(packetProcessorType);
                processor.ProcessPacket(packet, connection);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Received invalid, unauthenticated packet: {packet}");
            }
        }
    }
}
