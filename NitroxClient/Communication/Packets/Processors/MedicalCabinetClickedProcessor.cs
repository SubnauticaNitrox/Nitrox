using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel.Helper;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class MedicalCabinetClickedProcessor : ClientPacketProcessor<MedicalCabinetClicked>
    {
        public override void Process(MedicalCabinetClicked packet)
        {
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(packet.Guid);

            if(opGameObject.IsPresent())
            {
                GameObject gameObject = opGameObject.Get();
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
            else
            {
                Console.WriteLine("Could not locate medical cabinet with guid: " + packet.Guid);
            }
        }
    }
}
