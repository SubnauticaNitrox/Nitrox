using NitroxModel.Packets;
using NitroxServer.Communication;
using UnityEngine;

namespace NitroxServer.GameLogic
{
    public class AI
    {
        private int CHUNK_SIZE = 16; //find common place

        private TcpServer tcpServer;

        public AI(TcpServer tcpServer)
        {
            this.tcpServer = tcpServer;
        }

        public void CreatureActionChanged(Vector3 creaturePosition, CreatureAction newAction)
        {
            CreatureActionChanged actionChanged = new CreatureActionChanged(newAction.GetType().ToString());

            // TODO: Is this calculation incorrect just like in LoadedChunks.GetChunk?
            // TODO: Should this method be moved to NitroxModel for general usage?
            Int3 int3 = LargeWorldStreamer.main.GetContainingBatch(creaturePosition);
            //new Int3((int)Math.Floor(creaturePosition.x / CHUNK_SIZE) * CHUNK_SIZE,
            //                 (int)Math.Floor(creaturePosition.y / CHUNK_SIZE) * CHUNK_SIZE,
            //                 (int)Math.Floor(creaturePosition.z / CHUNK_SIZE) * CHUNK_SIZE);

            tcpServer.SendPacketToPlayersInChunk(actionChanged, int3);
        }
    }
}
