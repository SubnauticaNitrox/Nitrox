using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class MedicalCabinetClicked : PlayerActionPacket
    {
        public String Guid { get; private set; }
        public bool DoorOpen { get; private set; }
        public bool HasMedKit { get; private set; }
        public float NextSpawnTime { get; private set; }

        public MedicalCabinetClicked(String playerId, String guid, Vector3 actionPosition, bool doorOpen, bool hasMedKit, float nextSpawnTime) : base(playerId, actionPosition)
        {
            this.Guid = guid;
            this.DoorOpen = doorOpen;
            this.HasMedKit = hasMedKit;
            this.NextSpawnTime = nextSpawnTime;
        }

        public override string ToString()
        {
            return "[MedicalCabinetClicked - playerId: " + PlayerId + " guid: " + Guid + " DoorOpen: " + DoorOpen + " HasMedKit: " + HasMedKit + " NextSpawnTime: " + NextSpawnTime + "]";
        }
    }
}
