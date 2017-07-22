using NitroxModel.Packets;
using NitroxServer.Communication;
using NitroxServer.Communication.Packets.Processors;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using System;
using System.Collections.Generic;

namespace NitroxServer
{
    public class Server
    {
        private TcpServer tcpServer;
        private TimeKeeper timeKeeper;
        private Dictionary<Type, ServerPacketProcessor> packetProcessorsByType;
        
        public Server()
        {
            this.timeKeeper = new TimeKeeper();
            this.tcpServer = new TcpServer();
            this.packetProcessorsByType = new Dictionary<Type, ServerPacketProcessor>() {
                {typeof(Authenticate), new AuthenticatePacketProcessor(tcpServer, timeKeeper) },
            };
        }

        public void Start()
        {
            tcpServer.Start(packetProcessorsByType);
        }
    }
}
