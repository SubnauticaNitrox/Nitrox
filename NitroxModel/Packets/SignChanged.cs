using System;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SignChanged : Packet
    {
        public SignMetadata SignMetadata { get; }

        public SignChanged(SignMetadata signMetadata)
        {
            SignMetadata = signMetadata;
        }
    }
}
