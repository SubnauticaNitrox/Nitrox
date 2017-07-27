using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleMovement : Movement
    {
        public String TechType { get; protected set; }
        public String Guid { get; protected set; }

        public VehicleMovement(String playerId, Vector3 playerPosition, Quaternion rotation, String techType, String guid) : base(playerId, playerPosition, rotation, rotation, Optional<String>.Empty())
        {
            this.Guid = guid;
            this.TechType = techType;
            this.PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[VehicleMovement - TechType: " + TechType + " guid: " + Guid + "]\n\t" + base.ToString();
        }
    }
}
