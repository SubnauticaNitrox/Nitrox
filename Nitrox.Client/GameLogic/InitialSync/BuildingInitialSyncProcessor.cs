using System;
using System.Collections;
using System.Collections.Generic;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.GameLogic.Bases;
using Nitrox.Client.GameLogic.Bases.Spawning;
using Nitrox.Client.GameLogic.InitialSync.Base;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using UnityEngine;

namespace Nitrox.Client.GameLogic.InitialSync
{
    public class BuildingInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly IPacketSender packetSender;
        private readonly BuildThrottlingQueue buildEventQueue;
        private readonly BasePieceSpawnPrioritizer basePieceSpawnPrioritizer;

        private bool completed;
        
        public BuildingInitialSyncProcessor(IPacketSender packetSender, BuildThrottlingQueue buildEventQueue, BasePieceSpawnPrioritizer basePieceSpawnPrioritizer)
        {
            this.packetSender = packetSender;
            this.buildEventQueue = buildEventQueue;
            this.basePieceSpawnPrioritizer = basePieceSpawnPrioritizer;

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
                List<BasePiece> prioritizedBasePieces = basePieceSpawnPrioritizer.OrderBasePiecesByPriority(basePieces);
                QueueUpPieces(prioritizedBasePieces);
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
