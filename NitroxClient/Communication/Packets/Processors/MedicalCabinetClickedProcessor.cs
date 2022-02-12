using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class MedicalCabinetClickedProcessor : ClientPacketProcessor<MedicalCabinetClicked>
    {
        public override void Process(MedicalCabinetClicked packet)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
            MedicalCabinet cabinet = gameObject.RequireComponent<MedicalCabinet>();

            bool medkitPickedUp = !packet.HasMedKit && cabinet.hasMedKit;

            cabinet.hasMedKit = packet.HasMedKit;
            cabinet.timeSpawnMedKit = packet.NextSpawnTime;

            bool isDoorOpen = cabinet.doorOpen;
            bool doorChangedState = isDoorOpen != packet.DoorOpen;

            if (doorChangedState)
            {
                cabinet.Invoke(nameof(MedicalCabinet.ToggleDoorState), 0f);
            }
            else if (medkitPickedUp)
            {
                cabinet.Invoke(nameof(MedicalCabinet.ToggleDoorState), 1.8f);
            }
        }
    }
}
