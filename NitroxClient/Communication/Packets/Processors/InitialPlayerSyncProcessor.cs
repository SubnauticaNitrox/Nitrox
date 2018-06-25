using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using System.Collections.Generic;

namespace NitroxClient.Communication.Packets.Processors
{
    public class InitialPlayerSyncProcessor : ClientPacketProcessor<InitialPlayerSync>
    {
        private readonly BuildThrottlingQueue buildEventQueue;
        private readonly Vehicles vehicles;

        public InitialPlayerSyncProcessor(BuildThrottlingQueue buildEventQueue, Vehicles vehicles)
        {
            this.buildEventQueue = buildEventQueue;
            this.vehicles = vehicles;
        }

        public override void Process(InitialPlayerSync packet)
        {
            SpawnBasePieces(packet.BasePieces);
            SpawnVehicles(packet.Vehicles);
        }

        private void SpawnBasePieces(List<BasePiece> basePieces)
        {
            Log.Info("Received initial sync packet with " + basePieces.Count + " base pieces");

            foreach (BasePiece basePiece in basePieces)
            {
                buildEventQueue.EnqueueBasePiecePlaced(basePiece);

                if (basePiece.ConstructionCompleted)
                {
                    buildEventQueue.EnqueueConstructionCompleted(basePiece.Guid, basePiece.NewBaseGuid);
                }
                else
                {
                    buildEventQueue.EnqueueAmountChanged(basePiece.Guid, basePiece.ConstructionAmount);
                }
            }
        }

        private void SpawnVehicles(List<VehicleModel> vehicleModels)
        {
            Log.Info("Received initial sync packet with " + vehicleModels.Count + " vehicles");

            foreach (VehicleModel vehicle in vehicleModels)
            {
                vehicles.UpdateVehiclePosition(vehicle, Optional<RemotePlayer>.Empty());
            }
        }
    }
}
