using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    public enum ExosuitArmAction
    {
        pickup,
        punch
    }

    [Serializable]
    public class ExosuitArmActionPacket : Packet
    {
        public TechType TechType { get; }
        public string ArmGuid { get; }
        public ExosuitArmAction ArmAction { get; }

        public ExosuitArmActionPacket(TechType techType, string armGuid, ExosuitArmAction armAction)
        {
            TechType = techType;
            ArmGuid = armGuid;
            ArmAction = armAction;
        }

        public override string ToString()
        {
            return "[ExosuitArmAction - TechType: " + TechType + " Guid:" + ArmGuid + " ArmAction: " + ArmAction + "]";
        }
    }
}
