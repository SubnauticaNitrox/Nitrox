using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync
{
    public class BuildingInitialSyncProcessor : AsyncInitialSyncProcessor
    {
        private readonly IPacketSender packetSender;
        private readonly BuildThrottlingQueue buildEventQueue;
        
        public BuildingInitialSyncProcessor(IPacketSender packetSender, BuildThrottlingQueue buildEventQueue)
        {
            this.packetSender = packetSender;
            this.buildEventQueue = buildEventQueue;

            DependentProcessors.Add(typeof(CyclopsInitialAsyncProcessor));
        }

        public override void Process(InitialPlayerSync packet)
        {
            List<BasePiece> basePieces = packet.BasePieces;
            Log.Instance.LogMessage(LogCategory.Info, "Received initial sync packet with " + basePieces.Count + " base pieces");

            if (basePieces.Count == 0)
            {
                MarkCompleted();
            }
            else
            {
                QueueUpPieces(packet.BasePieces);
                ThrottledBuilder.main.QueueDrained += FinishedBuildingBasePieces;
            }
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

        private void FinishedBuildingBasePieces(object sender, EventArgs eventArgs)
        {
            MarkCompleted();
        }
    }
}
