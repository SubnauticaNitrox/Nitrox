using NitroxModel.Packets;
using System;
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
