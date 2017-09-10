using NitroxModel.Tcp;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class Packet
    {
        private static BinaryFormatter serializer = new BinaryFormatter();

        public byte[] SerializeWithHeaderData()
        {
            byte[] packetData;

            using (MemoryStream ms = new MemoryStream())
            {
                //place holder for size, will be filled in later... allows us
                //to avoid doing a byte array merge... zomg premature optimization
                ms.Write(new Byte[MessageBuffer.HEADER_BYTE_SIZE], 0, MessageBuffer.HEADER_BYTE_SIZE);
                serializer.Serialize(ms, this);
                packetData = ms.ToArray();
            }

            int packetSize = packetData.Length - MessageBuffer.HEADER_BYTE_SIZE; // subtract HEADER_BYTE_SIZE because we dont want to take into account the added bytes
            byte[] packetSizeBytes = BitConverter.GetBytes(packetSize);

            //premature optimization continued :)
            for(int i = 0; i < MessageBuffer.HEADER_BYTE_SIZE; i++)
            {
                packetData[i] = packetSizeBytes[i];
            }

            return packetData;
        }
    }
}
