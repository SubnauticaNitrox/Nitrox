using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures;
using NitroxModel.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NitroxModel.Packets.Packet;

namespace NitroxModel.Packets
{
    [Serializable]
    public class BasicMovement : Movement
    {
        public override NitroxId Id { get; }
        public override NitroxVector3 Position { get; }
        public override NitroxQuaternion Rotation { get; }

        public BasicMovement(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation)
        {
            Id = id;
            Position = position;
            Rotation = rotation;
            DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
            UdpChannel = UdpChannelId.MISC_MOVEMENT;
        }
    }
}