using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public string ArmGuid { get; }
        public ExosuitArmAction ArmAction { get; }
        public Optional<Vector3> OpVector { get; }
        public Optional<Quaternion> OpRotation { get; }

        public ExosuitArmActionPacket(TechType techType, string armGuid, ExosuitArmAction armAction, Optional<Vector3> opVector = null, Optional<Quaternion> opRotation = null)
        {
            TechType = techType;
            ArmGuid = armGuid;
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
            return "[ExosuitArmAction - TechType: " + TechType + " Guid:" + ArmGuid + " ArmAction: " + ArmAction + "vector: " + OpVector + " rotation: " + OpRotation + "]";
        }
    }
}
