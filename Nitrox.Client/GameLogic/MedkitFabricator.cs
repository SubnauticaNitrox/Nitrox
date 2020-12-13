using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Packets;

namespace Nitrox.Client.GameLogic
{
    public class MedkitFabricator
    {
        private readonly IPacketSender packetSender;

        public MedkitFabricator(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Clicked(MedicalCabinet medicalCabinet)
        {
            NitroxId id = NitroxEntity.GetId(medicalCabinet.gameObject);
            bool doorOpen = (bool)medicalCabinet.ReflectionGet("doorOpen");

            MedicalCabinetClicked cabinetClicked = new MedicalCabinetClicked(id, doorOpen, medicalCabinet.hasMedKit, medicalCabinet.timeSpawnMedKit);
            packetSender.Send(cabinetClicked);
        }
    }
}
