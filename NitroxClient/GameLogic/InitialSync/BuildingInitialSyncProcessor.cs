using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    public class BuildingInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly IPacketSender packetSender;
        private readonly BuildThrottlingQueue buildEventQueue;

        private bool completed;
        
        public BuildingInitialSyncProcessor(IPacketSender packetSender, BuildThrottlingQueue buildEventQueue)
        {
            this.packetSender = packetSender;
            this.buildEventQueue = buildEventQueue;

            DependentProcessors.Add(typeof(CyclopsInitialAsyncProcessor));
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            completed = false;

            List<BasePiece> basePieces = packet.BasePieces;
            Log.Info("Received initial sync packet with " + basePieces.Count + " base pieces");

            if (basePieces.Count == 0)
            {
                completed = true;
            }
            else
            {
                QueueUpPieces(packet.BasePieces);
                ThrottledBuilder.main.QueueDrained += FinishedCompletedBuildings;
            }

            yield return new WaitUntil(() => completed);
        }

        private void QueueUpPieces(List<BasePiece> basePieces)
        {
            using (packetSender.Suppress<ConstructionAmountChanged>())
            using (packetSender.Suppress<ConstructionCompleted>())
            using (packetSender.Suppress<PlaceBasePiece>())
            {
                foreach (BasePiece basePiece in basePieces)
                {
                    buildEventQueue.EnqueueBasePiecePlaced(basePiece);

                    if (basePiece.ConstructionCompleted)
                    {
                        buildEventQueue.EnqueueConstructionCompleted(basePiece.Id, basePiece.BaseId);
                    }
                    else
                    {
                        buildEventQueue.EnqueueAmountChanged(basePiece.Id, basePiece.ConstructionAmount);
                    }
                }
            }
        }

        private void FinishedCompletedBuildings(object sender, EventArgs eventArgs)
        {
            ThrottledBuilder.main.QueueDrained -= FinishedCompletedBuildings;
            completed = true;
        }
    }
}
