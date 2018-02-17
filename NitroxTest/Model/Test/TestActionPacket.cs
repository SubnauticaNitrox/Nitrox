using System;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxTest.Model
{
    [Serializable]
    public class TestActionPacket : PlayerActionPacket
    {
        public TestActionPacket(Vector3 eventPosition) : base(eventPosition)
        {
        }
    }
}
