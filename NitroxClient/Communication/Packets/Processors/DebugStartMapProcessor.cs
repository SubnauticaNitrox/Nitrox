using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class DebugStartMapProcessor : IClientPacketProcessor<DebugStartMapPacket>
    {
        public Task Process(IPacketProcessContext context, DebugStartMapPacket packet)
        {
            foreach (NitroxVector3 position in packet.StartPositions)
            {
                GameObject prim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                prim.transform.position = new Vector3(position.X, position.Y + 10, position.Z);
            }

            return Task.CompletedTask;
        }
    }
}
