﻿using NitroxClient.Communication;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class MedkitFabricator
    {
        private readonly PacketSender packetSender;

        public MedkitFabricator(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Clicked(MedicalCabinet medicalCabinet)
        {
            Vector3 actionPosition = medicalCabinet.gameObject.transform.position;
            string guid = GuidHelper.GetGuid(medicalCabinet.gameObject);
            bool doorOpen = (bool)medicalCabinet.ReflectionGet("doorOpen");

            MedicalCabinetClicked cabinetClicked = new MedicalCabinetClicked(guid, actionPosition, doorOpen, medicalCabinet.hasMedKit, medicalCabinet.timeSpawnMedKit);
            packetSender.Send(cabinetClicked);
        }
    }
}
