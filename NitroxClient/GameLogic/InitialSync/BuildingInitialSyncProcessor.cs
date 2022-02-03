using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Bases.Spawning.BasePiece;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    public class BuildingInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly IPacketSender packetSender;
        private readonly BuildThrottlingQueue buildEventQueue;
        private readonly BasePieceSpawnPrioritizer basePieceSpawnPrioritizer;

        private bool completed;

        /*
         * Certain objects like water parks need a frame to process internal stuff that will spawn or rebuild some parts of it
         * Therefore, we need these parts to have a later construction completed event
         */
        public static readonly List<TechType> LaterConstructionTechTypes = new() { TechType.BaseWaterPark };

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
                IEnumerable<BasePiece> prioritizedBasePieces = basePieceSpawnPrioritizer.OrderBasePiecesByPriority(basePieces);
                QueueUpPieces(prioritizedBasePieces);
                ThrottledBuilder.main.QueueDrained += FinishedCompletedBuildings;
            }

            yield return new WaitUntil(() => completed);
        }

        private void QueueUpPieces(IEnumerable<BasePiece> basePieces)
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
                        if (LaterConstructionTechTypes.Contains(basePiece.TechType.ToUnity()))
                        {
                            buildEventQueue.EnqueueLaterConstructionCompleted(basePiece.Id, basePiece.BaseId);
                        }
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
