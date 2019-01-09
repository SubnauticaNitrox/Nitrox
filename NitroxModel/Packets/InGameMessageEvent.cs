using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class InGameMessageEvent : Packet
    {
        public string Message { get; set; }

        public InGameMessageEvent(string message)
        {
            Message = message;
        }
    }
}
