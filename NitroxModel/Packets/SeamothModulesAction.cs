using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class SeamothModulesAction : Packet
    {
        [Index(0)]
        public virtual NitroxTechType TechType { get; protected set; }
        [Index(1)]
        public virtual int SlotID { get; protected set; }
        [Index(2)]
        public virtual NitroxId Id { get; protected set; }
        [Index(3)]
        public virtual NitroxVector3 Forward { get; protected set; }
        [Index(4)]
        public virtual NitroxQuaternion Rotation { get; protected set; }

        private SeamothModulesAction() { }

        public SeamothModulesAction(NitroxTechType techType, int slotID, NitroxId id, NitroxVector3 forward, NitroxQuaternion rotation)
        {
            TechType = techType;
            SlotID = slotID;
            Id = id;
            Forward = forward;
            Rotation = rotation;
        }
    }
}
