using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ExosuitModulesAction : Packet
    {
        public TechType TechType { get; }
        public int SlotID { get; }
        public string Guid { get; }
        public Vector3 Forward { get; }
        public Quaternion Rotation { get; }

        public ExosuitModulesAction(TechType techType, int slotID, string guid, Vector3 forward, Quaternion rotation)
        {
            TechType = techType;
            SlotID = slotID;
            Guid = guid;
            Forward = forward;
            Rotation = rotation;
        }
        public override string ToString()
        {
            return "[ExosuitModulesAction - TechType: " + TechType + " SlotID: " + SlotID + " Guid:" + Guid + " Forward: " + Forward + " Rotation: " + Rotation + "]";
        }
    }
}
