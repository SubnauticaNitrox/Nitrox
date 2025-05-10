using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class MedicalCabinetClickedProcessor : IClientPacketProcessor<MedicalCabinetClicked>
{
    public Task Process(IPacketProcessContext context, MedicalCabinetClicked packet)
    {
        GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
        MedicalCabinet cabinet = gameObject.RequireComponent<MedicalCabinet>();

        bool medkitPickedUp = !packet.HasMedKit && cabinet.hasMedKit;
        bool doorChangedState = cabinet.doorOpen != packet.DoorOpen;

        cabinet.hasMedKit = packet.HasMedKit;
        cabinet.timeSpawnMedKit = packet.NextSpawnTime;

        using (PacketSuppressor<FMODCustomEmitterPacket>.Suppress())
        using (FMODSystem.SuppressSubnauticaSounds())
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

        return Task.CompletedTask;
    }
}
