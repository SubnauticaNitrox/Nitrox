using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets
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
    }
}
