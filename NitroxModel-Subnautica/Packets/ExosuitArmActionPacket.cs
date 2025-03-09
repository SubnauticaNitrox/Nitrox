using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
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
        public NitroxVector3? OpVector { get; }
        public NitroxQuaternion? OpRotation { get; }

        public ExosuitArmActionPacket(TechType techType, NitroxId exosuitId, Exosuit.Arm armSide, ExosuitArmAction armAction, NitroxVector3? opVector, NitroxQuaternion? opRotation)
        {
            TechType = techType;
            ExosuitId = exosuitId;
            ArmSide = armSide;
            ArmAction = armAction;
            OpVector = opVector;
            OpRotation = opRotation;
        }

        public override string ToString()
        {
            return $"[ExosuitArmAction - TechType: {TechType}, ExosuitId:{ExosuitId}, ArmSide: {ArmSide}, ArmAction: {ArmAction}, Vector: {OpVector}, Rotation: {OpRotation}]";
        }
    }

    public enum ExosuitArmAction
    {
        START_USE_TOOL,
        END_USE_TOOL,
        ALT_HIT
    }
}
