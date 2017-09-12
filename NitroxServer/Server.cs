using NitroxModel.DataStructures.Util;
using NitroxServer.Communication;
using NitroxServer.Communication.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Threading;
using System;
using UnityEngine;

namespace NitroxServer
{
    public class Server : MonoBehaviour
    {
        public static bool ALLOW_MAP_CLIPPING = false;
        public static Logic Logic { get; private set; }

        private TcpServer tcpServer;
        private TimeKeeper timeKeeper;
        private SimulationOwnership simulationOwnership;
        private PacketHandler packetHandler;
        private GameActionManager gameActionManager;

        public Server()
        {
            this.timeKeeper = new TimeKeeper();
            this.tcpServer = new TcpServer();
            this.simulationOwnership = new SimulationOwnership();
            this.gameActionManager = new GameActionManager();
            this.packetHandler = new PacketHandler(tcpServer, timeKeeper, simulationOwnership, gameActionManager);

            Logic = new Logic(tcpServer);
        }

        public void Awake()
        {
            tcpServer.Start(packetHandler);
        }

        public void Update()
        {
            Optional<IGameAction> action = gameActionManager.next();

            while(action.IsPresent())
            {
                action.Get().Execute();
                action = gameActionManager.next();
            }
        }
    }
}
