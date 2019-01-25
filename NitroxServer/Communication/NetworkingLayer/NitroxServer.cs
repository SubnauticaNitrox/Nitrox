using System.Collections.Generic;
using NitroxServer.Communication.Packets;
using NitroxServer.ConfigParser;
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

        public NitroxServer(PacketHandler packetHandler, PlayerManager playerManager, EntitySimulation entitySimulation, ServerConfig serverConfig)
        {
            this.packetHandler = packetHandler;
            this.playerManager = playerManager;
            this.entitySimulation = entitySimulation;

            portNumber = serverConfig.ServerPort;
            maxConn = serverConfig.MaxConnections;
        }

        public abstract void Start();

        public abstract void Stop();
    }
}
