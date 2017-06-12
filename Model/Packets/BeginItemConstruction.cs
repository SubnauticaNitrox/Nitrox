using NitroxModel.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class BeginItemConstruction : PlayerActionPacket
    {
        public Vector3 ItemPosition { get; private set; }
        public Quaternion Rotation { get; private set; }
        public String TechType { get; private set; }

        public BeginItemConstruction(String playerId, Vector3 playerPosition, Vector3 itemPosition, Quaternion rotation, String techType) : base(playerId, playerPosition)
        {
            this.ItemPosition = itemPosition;
            this.Rotation = rotation;
            this.TechType = techType;
        }

        public override string ToString()
        {
            return "[Build Item - ItemPosition: " + ItemPosition + " GameObjectName: " + Rotation + " TechType: " + TechType + "]";
        }
    }
}
