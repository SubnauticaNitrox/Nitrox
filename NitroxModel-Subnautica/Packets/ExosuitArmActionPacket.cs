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
        pickup,
        punch,
        startUseTool,
        endUseTool,
        onHit
    }

    [Serializable]
    public class ExosuitArmActionPacket : Packet
    {
        public TechType TechType { get; }
        public string ArmGuid { get; }
        public ExosuitArmAction ArmAction { get; }
        public Optional<Vector3> OpVector { get; }

        public ExosuitArmActionPacket(TechType techType, string armGuid, ExosuitArmAction armAction, Optional<Vector3> opVector)
        {
            TechType = techType;
            ArmGuid = armGuid;
            ArmAction = armAction;
            OpVector = opVector;
            if(OpVector == null)
            {
                OpVector = Optional<Vector3>.Empty();
            }
        }

        public override string ToString()
        {
            return "[ExosuitArmAction - TechType: " + TechType + " Guid:" + ArmGuid + " ArmAction: " + ArmAction + "vector: " + OpVector + "]";
        }
    }
}
