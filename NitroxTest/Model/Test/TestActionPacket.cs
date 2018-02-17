using System;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxTest.Model
{
    [Serializable]
    public class TestActionPacket : RangedPacket
    {
        public TestActionPacket(Vector3 eventPosition, int level) : base(eventPosition, level)
        {
        }
    }
}
