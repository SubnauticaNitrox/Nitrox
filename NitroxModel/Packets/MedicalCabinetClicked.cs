using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class MedicalCabinetClicked : Packet
    {
        public NitroxId Id { get; }
        public bool DoorOpen { get; }
        public bool HasMedKit { get; }
        public float NextSpawnTime { get; }

        public MedicalCabinetClicked(NitroxId id, bool doorOpen, bool hasMedKit, float nextSpawnTime)
        {
            Id = id;
            DoorOpen = doorOpen;
            HasMedKit = hasMedKit;
            NextSpawnTime = nextSpawnTime;
        }

        public override string ToString()
        {
            return $"[MedicalCabinetClicked - Id: {Id}, DoorOpen: {DoorOpen}, HasMedKit: {HasMedKit}, NextSpawnTime: {NextSpawnTime}]";
        }
    }
}
