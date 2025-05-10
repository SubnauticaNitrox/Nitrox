using System;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record PlayerSyncFinished : Packet
    {
        public PlayerSyncFinished()
        {
            
        }
    }
}
