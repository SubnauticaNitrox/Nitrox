using System;
using DTO = NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SeamothModulesAction : Packet
    {
        public SeamothModulesAction(DTO.TechType techType, int slotID, DTO.NitroxId id, DTO.Vector3 forward, DTO.Quaternion rotation)
        {
            TechType = techType;
            SlotID = slotID;
            Id = id;
            Forward = forward;
            Rotation = rotation;
        }

        public DTO.TechType TechType { get; }
        public int SlotID { get; }
        public DTO.NitroxId Id { get; }
        public DTO.Vector3 Forward { get; }
        public DTO.Quaternion Rotation { get; }

        public override string ToString()
        {
            return "[SeamothModulesAction - TechType: " + TechType + " SlotID: " + SlotID + " Id:" + Id + " Forward: " + Forward + " Rotation: " + Rotation + "]";
        }
    }
}
