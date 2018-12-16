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
using NitroxModel.Core;

namespace NitroxServer.Communication.Packets
{
    public class PacketHandler
    {
        public void Process(Packet packet, Connection connection)
        {
            Player player = NitroxServiceLocator.LocateService<PlayerManager>().GetPlayer(connection);

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
            try
            {
                Type serverPacketProcessorType = typeof(AuthenticatedPacketProcessor<>);
                Type packetType = packet.GetType();
                Type packetProcessorType = serverPacketProcessorType.MakeGenericType(packetType);

                PacketProcessor processor = (PacketProcessor)NitroxServiceLocator.LocateService(packetProcessorType);
                processor.ProcessPacket(packet, player);
            }
            catch
            {
                new DefaultServerPacketProcessor(NitroxServiceLocator.LocateService<PlayerManager>()).ProcessPacket(packet, player);
            }
        }

        private void ProcessUnauthenticated(Packet packet, Connection connection)
        {
            try
            {
                Type serverPacketProcessorType = typeof(UnauthenticatedPacketProcessor<>);
                Type packetType = packet.GetType();
                Type packetProcessorType = serverPacketProcessorType.MakeGenericType(packetType);

                PacketProcessor processor = (PacketProcessor)NitroxServiceLocator.LocateService(packetProcessorType);
                processor.ProcessPacket(packet, connection);
            }
            catch (Exception ex)
            {
                Log.Error("Exception: ", ex);
            }
        }
    }
}
