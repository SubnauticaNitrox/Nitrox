using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxServer.GameLogic
{
    public class AI
    {
        private TcpServer tcpServer;
        private HashSet<String> movedCreaturesGuids = new HashSet<String>();

        public AI(TcpServer tcpServer)
        {
            this.tcpServer = tcpServer;
        }

        public void CreatureActionChanged(Vector3 creaturePosition, CreatureAction newAction)
        {
            CreatureActionChanged actionChanged = new CreatureActionChanged(newAction.GetType().ToString());

            Int3 batchId = LargeWorldStreamer.main.GetContainingBatch(creaturePosition);

            Chunk chunk = new Chunk(batchId, 1); //TODO: what is the right level?  Cascade down maybe?
            tcpServer.SendPacketToPlayersInChunk(actionChanged, chunk);
        }

        public void CreatureMoved(String creatureGuid)
        {
            lock(movedCreaturesGuids)
            {
                movedCreaturesGuids.Add(creatureGuid);
            }
        }

        public void BroadcastMovedCreatures()
        {
            Dictionary<String, Transform> creatureGuidsWithTransform = new Dictionary<String, Transform>();

            lock(movedCreaturesGuids)
            {
                foreach(String creatureGuid in movedCreaturesGuids)
                {
                    Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(creatureGuid);
                    
                    if(opGameObject.IsPresent())
                    {
                        creatureGuidsWithTransform.Add(creatureGuid, opGameObject.Get().transform);
                    }
                }

                movedCreaturesGuids.Clear();
            }

            CreaturePositionsChanged creaturePositionsChanged = new CreaturePositionsChanged(creatureGuidsWithTransform);
            tcpServer.SendPacketToAllPlayers(creaturePositionsChanged);
        }
    }
}
