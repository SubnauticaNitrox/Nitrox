using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class MedicalCabinetClickedProcessor : ClientPacketProcessor<MedicalCabinetClicked>
{
    private readonly IPacketSender packetSender;

    public MedicalCabinetClickedProcessor(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }

    public override void Process(MedicalCabinetClicked packet)
    {
        GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
        MedicalCabinet cabinet = gameObject.RequireComponent<MedicalCabinet>();

        bool medkitPickedUp = !packet.HasMedKit && cabinet.hasMedKit;
        bool doorChangedState = cabinet.doorOpen != packet.DoorOpen;

        cabinet.hasMedKit = packet.HasMedKit;
        cabinet.timeSpawnMedKit = packet.NextSpawnTime;

        using (PacketSuppressor<PlayFMODCustomEmitter>.Suppress())
        using (FMODSystem.SuppressSounds())
        {
            if (doorChangedState)
            {
                cabinet.Invoke(nameof(MedicalCabinet.ToggleDoorState), 0f);
            }
            else if (medkitPickedUp)
            {
                cabinet.Invoke(nameof(MedicalCabinet.ToggleDoorState), 2f);
            }
        }
    }
}
