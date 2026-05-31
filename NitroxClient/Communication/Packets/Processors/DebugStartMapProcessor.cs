using System.Collections.Generic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class DebugStartMapProcessor : IClientPacketProcessor<DebugStartMapPacket>
{
    private readonly List<GameObject> activeCubes = [];

    public Task Process(ClientProcessorContext context, DebugStartMapPacket packet)
    {
        foreach (GameObject cube in activeCubes)
        {
            Object.Destroy(cube);
        }
        activeCubes.Clear();
        foreach (NitroxVector3 position in packet.StartPositions)
        {
            GameObject prim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prim.transform.position = new Vector3(position.X, position.Y + 10, position.Z);
            activeCubes.Add(prim);
        }
        return Task.CompletedTask;
    }
}
