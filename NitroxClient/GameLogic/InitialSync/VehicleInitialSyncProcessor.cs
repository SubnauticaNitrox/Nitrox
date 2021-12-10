using System.Collections;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.InitialSync
{
    public class VehicleInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly Vehicles vehicles;
        private readonly IPacketSender packetSender;

        public VehicleInitialSyncProcessor(Vehicles vehicles, IPacketSender packetSender)
        {
            this.vehicles = vehicles;
            this.packetSender = packetSender;

            DependentProcessors.Add(typeof(BuildingInitialSyncProcessor));
            DependentProcessors.Add(typeof(CyclopsInitialAsyncProcessor));
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            int totalSyncedVehicles = 0;
            int nonCyclopsVehicleCount = packet.Vehicles.Where(v => v.TechType.ToUnity() != TechType.Cyclops).Count();

            foreach (VehicleModel vehicle in packet.Vehicles)
            {
                if (vehicle.TechType.ToUnity() != TechType.Cyclops)
                {
                    using (packetSender.Suppress<VehicleDocking>())
                    {
                        waitScreenItem.SetProgress(totalSyncedVehicles, nonCyclopsVehicleCount);
                        vehicles.CreateVehicle(vehicle);
                        totalSyncedVehicles++;
                        yield return null;
                    }
                }
            }

            Log.Info("Recieved initial sync with " + totalSyncedVehicles + " non-cyclops vehicles");
        }
    }
}
