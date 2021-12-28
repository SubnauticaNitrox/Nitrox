using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class MedicalCabinetClicked : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual bool DoorOpen { get; protected set; }
        [Index(2)]
        public virtual bool HasMedKit { get; protected set; }
        [Index(3)]
        public virtual float NextSpawnTime { get; protected set; }

        public MedicalCabinetClicked() { }

        public MedicalCabinetClicked(NitroxId id, bool doorOpen, bool hasMedKit, float nextSpawnTime)
        {
            Id = id;
            DoorOpen = doorOpen;
            HasMedKit = hasMedKit;
            NextSpawnTime = nextSpawnTime;
        }
    }
}
