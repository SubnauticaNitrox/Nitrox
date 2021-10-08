using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SeamothModulesAction : Packet
    {
        public NitroxTechType TechType { get; }
        public int SlotID { get; }
        public NitroxId Id { get; }
        public NitroxVector3 Forward { get; }
        public NitroxQuaternion Rotation { get; }

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
