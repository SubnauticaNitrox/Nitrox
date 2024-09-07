using System;
namespace NitroxModel.Packets
{
    [Serializable]
    public class FootstepPacket : Packet
    {
        public ushort playerID { get; }
        public StepSounds assetIndex { get; }
        public FootstepPacket(ushort playerID, StepSounds assetIndex)
        {
            this.playerID = playerID;
            this.assetIndex = assetIndex;
        }
        public enum StepSounds : byte
        {
            PRECURSOR_STEP_SOUND,
            METAL_STEP_SOUND,
            LAND_STEP_SOUND
        }
    }
}
