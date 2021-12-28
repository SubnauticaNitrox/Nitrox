using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Packets;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    [ZeroFormattable]
    public class ExosuitArmActionPacket : Packet
    {
        [Index(0)]
        public virtual NitroxTechType TechType { get; protected set; }
        [Index(1)]
        public virtual NitroxId ArmId { get; protected set; }
        [Index(2)]
        public virtual ExosuitArmAction ArmAction { get; protected set; }
        [Index(3)]
        public virtual NitroxVector3? OpVector { get; protected set; }
        [Index(4)]
        public virtual NitroxQuaternion? OpRotation { get; protected set; }

        public ExosuitArmActionPacket() { }

        public ExosuitArmActionPacket(NitroxTechType techType, NitroxId armId, ExosuitArmAction armAction, NitroxVector3? opVector, NitroxQuaternion? opRotation)
        {
            TechType = techType;
            ArmId = armId;
            ArmAction = armAction;
            OpVector = opVector;
            OpRotation = opRotation;
        }

        public override string ToString()
        {
            return $"[ExosuitArmAction - TechType: {TechType}, ArmId:{ArmId}, ArmAction: {ArmAction}, Vector: {OpVector}, Rotation: {OpRotation}]";
        }
    }

    public enum ExosuitArmAction
    {
        START_USE_TOOL,
        END_USE_TOOL,
        ALT_HIT
    }
}
