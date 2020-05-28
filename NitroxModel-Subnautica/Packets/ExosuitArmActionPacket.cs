using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxModel_Subnautica.Packets
{
    public enum ExosuitArmAction
    {
        START_USE_TOOL,
        END_USE_TOOL,
        ALT_HIT
    }

    [Serializable]
    public class ExosuitArmActionPacket : Packet
    {
        public TechType TechType { get; }
        public NitroxId ArmId { get; }
        public ExosuitArmAction ArmAction { get; }
        public Vector3? OpVector { get; }
        public Quaternion? OpRotation { get; }

        public ExosuitArmActionPacket(TechType techType, NitroxId armId, ExosuitArmAction armAction, Vector3? opVector, Quaternion? opRotation)
        {
            TechType = techType;
            ArmId = armId;
            ArmAction = armAction;
            OpVector = opVector;
            OpRotation = opRotation;
        }

        public override string ToString()
        {
            return "[ExosuitArmAction - TechType: " + TechType + " ArmId:" + ArmId + " ArmAction: " + ArmAction + "vector: " + OpVector + " rotation: " + OpRotation + "]";
        }
    }
}
