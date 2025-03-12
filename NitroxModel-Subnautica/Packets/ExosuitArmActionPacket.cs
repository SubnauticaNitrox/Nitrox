using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class ExosuitArmActionPacket : Packet
    {
        public TechType TechType { get; }
        public NitroxId ExosuitId { get; }
        public Exosuit.Arm ArmSide { get; }
        public ExosuitArmAction ArmAction { get; }

        public ExosuitArmActionPacket(TechType techType, NitroxId exosuitId, Exosuit.Arm armSide, ExosuitArmAction armAction)
        {
            TechType = techType;
            ExosuitId = exosuitId;
            ArmSide = armSide;
            ArmAction = armAction;
        }

        public override string ToString()
        {
            return $"[ExosuitArmAction - TechType: {TechType}, ExosuitId:{ExosuitId}, ArmSide: {ArmSide}, ArmAction: {ArmAction}]";
        }
    }

    public enum ExosuitArmAction
    {
        START_USE_TOOL,
        END_USE_TOOL,
        ALT_HIT
    }
}
