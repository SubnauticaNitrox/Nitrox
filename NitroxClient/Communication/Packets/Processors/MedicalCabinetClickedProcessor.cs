using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel.Helper;
using System;
using UnityEngine;
using NitroxModel.Helper.GameLogic;

namespace NitroxClient.Communication.Packets.Processors
{
    public class MedicalCabinetClickedProcessor : ClientPacketProcessor<MedicalCabinetClicked>
    {
        public override void Process(MedicalCabinetClicked packet)
        {
            GameObject gameObject = GuidHelper.RequireObjectFrom(packet.Guid);            
            MedicalCabinet cabinet = gameObject.GetComponent<MedicalCabinet>();

            if(cabinet != null)
            {
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
            else
            {
                Console.WriteLine("Guid " + packet.Guid + " did not have a MedicalCabinet script");
            }
        }
    }
}
