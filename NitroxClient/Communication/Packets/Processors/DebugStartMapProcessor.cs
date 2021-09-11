using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class DebugStartMapProcessor : ClientPacketProcessor<DebugStartMapPacket>
    {
        public override void Process(DebugStartMapPacket packet)
        {
            foreach (NitroxVector3 position in packet.StartPositions)
            {
                GameObject prim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                prim.transform.position = new Vector3(position.X, position.Y + 10, position.Z);
            }
        }
    }
}
