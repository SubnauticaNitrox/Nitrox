using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel.Helper;
using UnityEngine;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Helper.Unity;

namespace NitroxClient.Communication.Packets.Processors
{
    public class MedicalCabinetClickedProcessor : ClientPacketProcessor<MedicalCabinetClicked>
    {
        public override void Process(MedicalCabinetClicked packet)
        {
            GameObject gameObject = GuidHelper.RequireObjectFrom(packet.Guid);            
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
