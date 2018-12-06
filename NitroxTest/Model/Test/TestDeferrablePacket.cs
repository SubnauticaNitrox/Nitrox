using System;
using NitroxModel.Packets;
using UnityEngine;
using NitroxModel.DataStructures.Util;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxTest.Model
{
    [Serializable]
    public class TestDeferrablePacket : Packet
    {
        private Vector3 position;
        private int level;

        public TestDeferrablePacket(Vector3 position, int level)
        {
            this.position = position;
            this.level = level;
        }

        public override Optional<AbsoluteEntityCell> GetDeferredCell()
        {
            return Optional<AbsoluteEntityCell>.Of(new AbsoluteEntityCell(position, level));
        }
    }
}
