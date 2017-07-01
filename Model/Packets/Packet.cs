using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class Packet
    {
        public String PlayerId { get; protected set; }

        public Packet(String playerId)
        {
            this.PlayerId = playerId;
        }
        
        public byte[] SerializeWithHeaderData()
        {
            byte[] packetData;
            BinaryFormatter bf = new BinaryFormatter();

            using (MemoryStream ms = new MemoryStream())
            {
                //place holder for size, will be filled in later... allows us
                //to avoid doing a byte array merge... zomg premature optimization
                ms.Write(new Byte[] { 0x00, 0x00 }, 0, 2);
                bf.Serialize(ms, this);
                packetData = ms.ToArray();
            }

            Int16 packetSize = (Int16)(packetData.Length - 2); // subtract 2 because we dont want to take into account the added bytes
            byte[] packetSizeBytes = BitConverter.GetBytes(packetSize);

            //premature optimization continued :)
            packetData[0] = packetSizeBytes[0];
            packetData[1] = packetSizeBytes[1];

            return packetData;
        }
    }
}
