using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
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
            bool doorOpen = medicalCabinet.doorOpen;

            MedicalCabinetClicked cabinetClicked = new(id, doorOpen, medicalCabinet.hasMedKit, medicalCabinet.timeSpawnMedKit);
            packetSender.Send(cabinetClicked);
        }
    }
}
