using System;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxTest.Model
{
    [Serializable]
    public class TestDeferrablePacket : DeferrablePacket
    {
        public TestDeferrablePacket(Vector3 eventPosition, int level) : base(eventPosition, level)
        {
        }
    }
}
