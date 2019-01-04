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
using NitroxModel.DataStructures.Util;

namespace NitroxServer.Communication.Packets
{
    public class PacketHandler
    {
        private PlayerManager playerManager;
        private DefaultServerPacketProcessor defaultServerPacketProcessor;

        public PacketHandler(PlayerManager playerManager, DefaultServerPacketProcessor packetProcessor)
        {
            this.playerManager = playerManager;
            defaultServerPacketProcessor = packetProcessor;
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
            Type serverPacketProcessorType = typeof(AuthenticatedPacketProcessor<>);
            Type packetType = packet.GetType();
            Type packetProcessorType = serverPacketProcessorType.MakeGenericType(packetType);

            Optional<object> opProcessor = NitroxServiceLocator.LocateOptionalService(packetProcessorType);

            if (opProcessor.IsPresent())
            {
                PacketProcessor processor = (PacketProcessor)opProcessor.Get();
                processor.ProcessPacket(packet, player);
            }
            else
            {
                defaultServerPacketProcessor.ProcessPacket(packet, player);
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
                Log.Info("Received invalid, unauthenticated packet: " + packet);
                Log.Error("Exception:", ex);
            }
        }
    }
}
