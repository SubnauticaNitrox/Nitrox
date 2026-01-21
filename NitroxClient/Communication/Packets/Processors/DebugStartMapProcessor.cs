using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal class DebugStartMapProcessor : IClientPacketProcessor<DebugStartMapPacket>
{
    public Task Process(ClientProcessorContext context, DebugStartMapPacket packet)
    {
        foreach (NitroxVector3 position in packet.StartPositions)
        {
            GameObject prim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prim.transform.position = new Vector3(position.X, position.Y + 10, position.Z);
        }
        return Task.CompletedTask;
    }
}