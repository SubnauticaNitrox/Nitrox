using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace NitroxModel.Tcp
{
    public class MessageBuffer
    {
        public const int HEADER_BYTE_SIZE = 2;
        public const int ReceivingBufferSize = 4096;
        public const int MessageConstructingBufferSize = Int16.MaxValue;
        
        public byte[] ReceivingBuffer { get; set; }
        private byte[] MessageConstructingBuffer { get; set; }

        private int MessageConstructingBufferPointer = 0;
        private Int16 CurrentPacketLength = 0;

        private IFormatter formatter = new BinaryFormatter();
        
        public MessageBuffer()
        {
            this.ReceivingBuffer = new byte[ReceivingBufferSize];
            this.MessageConstructingBuffer = new byte[MessageConstructingBufferSize];
        }

        public IEnumerable<Packet> GetReceivedPackets(int receivedDataLength)
        {
            int headerOffset = 0;

            if(ReadingHeaderData(receivedDataLength))
            {
                CurrentPacketLength = BitConverter.ToInt16(ReceivingBuffer, 0);
                headerOffset = HEADER_BYTE_SIZE;
            }

            for (int i = headerOffset; i < receivedDataLength; i++)
            {
                MessageConstructingBuffer[MessageConstructingBufferPointer] = ReceivingBuffer[i];
                MessageConstructingBufferPointer++;

                if (CurrentPacketLength == MessageConstructingBufferPointer)
                {
                    //deserialize this packet and add it to our queue of received packets
                    using (Stream stream = new MemoryStream(MessageConstructingBuffer.Take(CurrentPacketLength).ToArray()))
                    {
                        yield return (Packet)formatter.Deserialize(stream);
                    }

                    CurrentPacketLength = 0;
                    MessageConstructingBufferPointer = 0;

                    // Check to see if this message contains the header for the next message
                    if ((i + 2) < receivedDataLength)
                    {
                        //if so, set it and make i skip it.
                        CurrentPacketLength = BitConverter.ToInt16(ReceivingBuffer, i + 1);
                        i += 2;
                    }
                }
            }
        }

        private bool ReadingHeaderData(int receivedDataLength)
        {
            if (CurrentPacketLength == 0)
            {
                if (receivedDataLength < HEADER_BYTE_SIZE)
                {
                    throw new Exception("First piece of message must contain a " + HEADER_BYTE_SIZE + " byte message size");
                }

                return true;
            }

            return false;
        }
    }
}
