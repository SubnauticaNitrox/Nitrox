using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxTest.Model
{
    [Serializable]
    public class TestActionPacket : RangedPacket
    {
        public TestActionPacket(Vector3 eventPosition) : base(eventPosition)
        {
        }
    }
}
