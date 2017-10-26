using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class MedicalCabinetClicked : PlayerActionPacket
    {
        public String Guid { get; }
        public bool DoorOpen { get; }
        public bool HasMedKit { get; }
        public float NextSpawnTime { get; }

        public MedicalCabinetClicked(String playerId, String guid, Vector3 actionPosition, bool doorOpen, bool hasMedKit, float nextSpawnTime) : base(playerId, actionPosition)
        {
            Guid = guid;
            DoorOpen = doorOpen;
            HasMedKit = hasMedKit;
            NextSpawnTime = nextSpawnTime;
        }

        public override string ToString()
        {
            return "[MedicalCabinetClicked - playerId: " + PlayerId + " guid: " + Guid + " DoorOpen: " + DoorOpen + " HasMedKit: " + HasMedKit + " NextSpawnTime: " + NextSpawnTime + "]";
        }
    }
}
