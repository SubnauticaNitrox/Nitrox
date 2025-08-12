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

        DeliveryMethod = Networking.NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
        UdpChannel = UdpChannelId.MOVEMENTS;
    }

    public enum StepSounds : byte
    {
#if SUBNAUTICA
        PRECURSOR,
#endif
        METAL,
        LAND
    }
}
