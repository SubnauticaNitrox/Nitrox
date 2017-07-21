using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ServerInformation : Packet
    {
        public String Name { get; private set; }
        public int CurrentPlayers { get; private set; }
        public int MaxPlayers { get; private set; }
        public bool PasswordProtected { get; private set; }

        public ServerInformation() : base()
        {

        }
    }
}
