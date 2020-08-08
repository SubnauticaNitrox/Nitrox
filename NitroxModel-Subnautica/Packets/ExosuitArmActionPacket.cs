using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class ExosuitArmActionPacket : Packet
    {
        public TechType TechType { get; }
        public NitroxId ArmId { get; }
        public ExosuitArmAction ArmAction { get; }
        public Vector3? Vector { get; }
        public Quaternion? Rotation { get; }

        public ExosuitArmActionPacket(TechType techType, NitroxId armId, ExosuitArmAction armAction, Vector3? vector, Quaternion? rotation)
        {
            TechType = techType;
            ArmId = armId;
            ArmAction = armAction;
            Vector = vector;
            Rotation = rotation;
        }

        public override string ToString()
        {
            return $"[ExosuitArmAction - TechType: {TechType}, ArmId: {ArmId}, ArmAction: {ArmAction}, Vector: {Vector}, Rotation: {Rotation}]";
        }
    }

    public enum ExosuitArmAction
    {
        START_USE_TOOL,
        END_USE_TOOL,
        ALT_HIT
    }
}
