using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxModel_Subnautica.Packets
{
    public enum ExosuitArmAction
    {
        startUseTool,
        endUseTool,
        altHit
    }

    [Serializable]
    public class ExosuitArmActionPacket : Packet
    {
        public TechType TechType { get; }
        public NitroxId ArmId { get; }
        public ExosuitArmAction ArmAction { get; }
        public Optional<Vector3> OpVector { get; }
        public Optional<Quaternion> OpRotation { get; }

        public ExosuitArmActionPacket(TechType techType, NitroxId armId, ExosuitArmAction armAction, Optional<Vector3> opVector = null, Optional<Quaternion> opRotation = null)
        {
            TechType = techType;
            ArmId = armId;
            ArmAction = armAction;
            OpVector = opVector;
            if(OpVector == null)
            {
                OpVector = Optional<Vector3>.Empty();
            }
            OpRotation = opRotation;
            if(OpRotation == null)
            {
                OpRotation = Optional<Quaternion>.Empty();
            }
        }

        public override string ToString()
        {
            return "[ExosuitArmAction - TechType: " + TechType + " ArmId:" + ArmId + " ArmAction: " + ArmAction + "vector: " + OpVector + " rotation: " + OpRotation + "]";
        }
    }
}
