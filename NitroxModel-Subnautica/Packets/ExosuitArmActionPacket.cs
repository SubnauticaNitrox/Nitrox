using NitroxModel.DataStructures;
using NitroxModel.Packets;
using UnityEngine;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    [ZeroFormattable]
    public class ExosuitArmActionPacket : Packet // TODO: Edit this class to be consistent with other packets
    {
        [Index(0)]
        public virtual TechType TechType { get; protected set; }
        [Index(1)]
        public virtual NitroxId ArmId { get; protected set; }
        [Index(2)]
        public virtual ExosuitArmAction ArmAction { get; protected set; }
        [Index(3)]
        public virtual Vector3? OpVector { get; protected set; }
        [Index(4)]
        public virtual Quaternion? OpRotation { get; protected set; }

        private ExosuitArmActionPacket() { }

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
