using NitroxModel.Logger;
using NitroxModel.DataStructures.Util;
using NitroxServer.Communication;
using NitroxServer.Communication.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Monobehaviours;
using NitroxServer.GameLogic.Threading;
using UnityEngine;

namespace NitroxServer
{
    public class Server : MonoBehaviour
    {
        public static Logic Logic { get; private set; }

        private TcpServer tcpServer;
        private TimeKeeper timeKeeper;
        private SimulationOwnership simulationOwnership;
        private PacketHandler packetHandler;
        private GameActionManager gameActionManager;
        private ChunkManager chunkManager;

        public Server()
        {
            this.timeKeeper = new TimeKeeper();
            this.tcpServer = new TcpServer();
            this.simulationOwnership = new SimulationOwnership();
            this.gameActionManager = new GameActionManager();
            this.chunkManager = new ChunkManager();
            this.packetHandler = new PacketHandler(tcpServer, timeKeeper, simulationOwnership, gameActionManager, chunkManager);

            Logic = new Logic(tcpServer);
        }

        public void Awake()
        {
            tcpServer.Start(packetHandler);
            ChunkLoader chunkLoader = this.gameObject.AddComponent<ChunkLoader>();
            chunkLoader.chunkManager = chunkManager;

            this.gameObject.AddComponent<CreaturePositionBroadcaster>();
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
