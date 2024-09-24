using System;

namespace NitroxModel.Packets;

[Serializable]
public class FootstepPacket : Packet
{
    public ushort PlayerID { get; }
    public StepSounds AssetIndex { get; }

    public FootstepPacket(ushort playerID, StepSounds assetIndex)
    {
        this.PlayerID = playerID;
        this.AssetIndex = assetIndex;
    }

    public enum StepSounds : byte
    {
        PRECURSOR_STEP_SOUND,
        METAL_STEP_SOUND,
        LAND_STEP_SOUND
    }
}
