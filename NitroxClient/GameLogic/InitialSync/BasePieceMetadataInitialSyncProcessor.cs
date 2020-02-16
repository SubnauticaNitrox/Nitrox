using NitroxClient.GameLogic.Bases.Metadata;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync
{
    public class BasePieceMetadataInitialSyncProcessor : InitialSyncProcessor
    {
        public BasePieceMetadataInitialSyncProcessor()
        {
            DependentProcessors.Add(typeof(BuildingInitialSyncProcessor)); // Meta data augments base pieces so they must be spawned first.
        }

        public override void Process(InitialPlayerSync packet)
        {
            int basePieceMetadatas = 0;

            foreach (BasePiece basePiece in packet.BasePieces)
            {
                if (basePiece.Metadata.IsPresent())
                {
                    BasePieceMetadata metadata = basePiece.Metadata.Get();
                    BasePieceMetadataProcessor metadataProcessor = BasePieceMetadataProcessor.FromMetaData(metadata);
                    metadataProcessor.UpdateMetadata(basePiece.Id, metadata);

                    basePieceMetadatas++;
                }
            }

            Log.Info("Received initial sync packet with " + basePieceMetadatas + " base piece meta data");
        }
    }
}
