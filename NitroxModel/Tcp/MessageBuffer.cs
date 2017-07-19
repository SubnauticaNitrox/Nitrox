using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace NitroxModel.Tcp
{
    public class MessageBuffer
    {
        public const int HEADER_BYTE_SIZE = 2;
        public const int RECEIVING_BUFFER_SIZE = 8192;
        public const int MESSAGE_CONSTRUCTING_BUFFER_SIZE = Int16.MaxValue;
        
        public byte[] ReceivingBuffer { get; set; }

        private byte[] MessageConstructingBuffer;
        private int MessageConstructingBufferPointer = 0;
        private Int16 CurrentPacketLength = 0;
        private byte TruncatedLengthByte = 0;

        private IFormatter formatter = new BinaryFormatter();
        
        public MessageBuffer()
        {
            this.ReceivingBuffer = new byte[RECEIVING_BUFFER_SIZE];
            this.MessageConstructingBuffer = new byte[MESSAGE_CONSTRUCTING_BUFFER_SIZE];
        }

        public IEnumerable<Packet> GetReceivedPackets(int receivedDataLength)
        {
            int headerOffset = 0;

            if(ReadingHeaderData(receivedDataLength))
            {
                if(TruncatedLengthByte != 0)
                {
                    CurrentPacketLength = (short)((BitConverter.ToInt16(ReceivingBuffer, 0) << 8) + TruncatedLengthByte);
                    headerOffset = HEADER_BYTE_SIZE - 1;
                }
                else
                {
                    CurrentPacketLength = BitConverter.ToInt16(ReceivingBuffer, 0);
                    headerOffset = HEADER_BYTE_SIZE;
                }
                
                TruncatedLengthByte = 0;
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
                        TruncatedLengthByte = 0;
                        i += 2;
                    }
                    else if((i + 1) < receivedDataLength) //maybe it contains a partial header?
                    {
                        TruncatedLengthByte = ReceivingBuffer[i + 1];
                        i++;
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
