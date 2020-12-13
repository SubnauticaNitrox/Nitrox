using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.Helper;
using Nitrox.Model.Packets;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
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

            bool isDoorOpen = (bool)cabinet.ReflectionGet("doorOpen");
            bool doorChangedState = isDoorOpen != packet.DoorOpen;

            if (doorChangedState)
            {
                cabinet.Invoke("ToggleDoorState", 0f);
            }
            else if (medkitPickedUp)
            {
                cabinet.Invoke("ToggleDoorState", 1.8f);
            }
        }
    }
}
