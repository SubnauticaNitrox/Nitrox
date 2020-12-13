using System.Collections;
using Nitrox.Client.GameLogic.Bases.Metadata;
using Nitrox.Client.GameLogic.InitialSync.Base;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.GameLogic.Buildings.Metadata;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;

namespace Nitrox.Client.GameLogic.InitialSync
{
    public class BasePieceMetadataInitialSyncProcessor : InitialSyncProcessor
    {
        public BasePieceMetadataInitialSyncProcessor()
        {
            DependentProcessors.Add(typeof(BuildingInitialSyncProcessor)); // Meta data augments base pieces so they must be spawned first.
        }
        
        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            int basePiecesWithMetadata = 0;
            int basePiecesChecked = 0;

            foreach (BasePiece basePiece in packet.BasePieces)
            {
                waitScreenItem.SetProgress(basePiecesChecked, packet.BasePieces.Count);

                if (basePiece.Metadata.HasValue)
                {
                    BasePieceMetadata metadata = basePiece.Metadata.Value;
                    BasePieceMetadataProcessor metadataProcessor = BasePieceMetadataProcessor.FromMetaData(metadata);
                    metadataProcessor.UpdateMetadata(basePiece.Id, metadata);
                    basePiecesWithMetadata++;
                }

                basePiecesChecked++;
                yield return null;
            }

            Log.Info("Received initial sync packet having " + basePiecesWithMetadata + " base pieces with meta data");
        }
    }
}
