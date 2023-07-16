using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Bases.New;
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
        private int latestCount = 0;

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
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            yield break;
        }

        private void QueueUpPieces(IEnumerable<BasePiece> basePieces)
        {
            using (PacketSuppressor<ConstructionAmountChanged>.Suppress())
            using (PacketSuppressor<ConstructionCompleted>.Suppress())
            using (PacketSuppressor<PlaceBasePiece>.Suppress())
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
            completed = true;
        }

        private void UpdateProgress(int totalPieces, WaitScreen.ManualWaitItem waitScreenItem)
        {
            if (latestCount == buildEventQueue.Count)
            {
                return;
            }
            latestCount = buildEventQueue.Count;
            waitScreenItem.SetProgress(totalPieces - buildEventQueue.Count, totalPieces);
            WaitScreen.main.Update();
        }
    }
}
