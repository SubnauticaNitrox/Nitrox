using System;

namespace NitroxModel.Packets
{
    //This is a packet that we use to "ping" a server to let it know that we'd like more information 
    //on the current requirements to submit a reservation to the ongoing game session.
    [Serializable]
    public class MultiplayerPolicySessionRequest : Packet
    {
    }
}
