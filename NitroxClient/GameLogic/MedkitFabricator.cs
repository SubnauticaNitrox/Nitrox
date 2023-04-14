using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
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
            if (!NitroxEntity.TryGetIdOrWarn(medicalCabinet.gameObject, out NitroxId id))
            {
                return;
            }

            MedicalCabinetClicked cabinetClicked = new(id, medicalCabinet.doorOpen, medicalCabinet.hasMedKit, medicalCabinet.timeSpawnMedKit);
            packetSender.Send(cabinetClicked);
        }
    }
}
