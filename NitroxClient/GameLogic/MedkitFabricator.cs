using NitroxClient.Communication;
using NitroxModel.Packets;
using NitroxModel.Helper;
using System;
using UnityEngine;
using NitroxClient.GameLogic.Helper;

namespace NitroxClient.GameLogic
{
    public class MedkitFabricator
    {
        private PacketSender packetSender;

        public MedkitFabricator(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Clicked(MedicalCabinet medicalCabinet)
        {
            Vector3 actionPosition = medicalCabinet.gameObject.transform.position;
            String guid = GuidHelper.GetGuid(medicalCabinet.gameObject);
            bool doorOpen = (bool)medicalCabinet.ReflectionGet("doorOpen");

            MedicalCabinetClicked cabinetClicked = new MedicalCabinetClicked(packetSender.PlayerId, guid, actionPosition, doorOpen, medicalCabinet.hasMedKit, medicalCabinet.timeSpawnMedKit);
            packetSender.Send(cabinetClicked);
        }
    }
}
