using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.GameLogic;
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
