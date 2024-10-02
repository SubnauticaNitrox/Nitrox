using System;

namespace NitroxModel.Packets;

[Serializable]
public class FootstepPacket : Packet
{
    public ushort PlayerID { get; }
    public StepSounds AssetIndex { get; }

    public FootstepPacket(ushort playerID, StepSounds assetIndex)
    {
        PlayerID = playerID;
        AssetIndex = assetIndex;
    }

    public enum StepSounds : byte
    {
        PRECURSOR,
        METAL,
        LAND
    }
}
