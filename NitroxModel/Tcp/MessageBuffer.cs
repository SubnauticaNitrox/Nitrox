using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace NitroxModel.Tcp
{
    public class MessageBuffer
    {
        public const int HEADER_BYTE_SIZE = 3;
        public const int RECEIVING_BUFFER_SIZE = 8192;
        public const int MESSAGE_CONSTRUCTING_BUFFER_SIZE = 1000000; // 1MB

        public byte[] ReceivingBuffer { get; set; }

        private byte[] MessageConstructingBuffer;
        private int MessageConstructingBufferPointer = 0;
        private int CurrentPacketLength = 0;

        private int nextPacketHeaderBytesRead = 0;
        private byte[] nextPacketHeaderBytes = new byte[4];

        private IFormatter formatter = Packet.Serializer;

        public MessageBuffer()
        {
            this.ReceivingBuffer = new byte[RECEIVING_BUFFER_SIZE];
            this.MessageConstructingBuffer = new byte[MESSAGE_CONSTRUCTING_BUFFER_SIZE];
        }

        public IEnumerable<Packet> GetReceivedPackets(int receivedDataLength)
        {
            int headerOffset = 0;

            if (ReadingHeaderData(receivedDataLength))
            {
                if (nextPacketHeaderBytesRead != 0)
                {
                    for (int i = nextPacketHeaderBytesRead; i < HEADER_BYTE_SIZE; i++)
                    {
                        nextPacketHeaderBytes[i] = ReceivingBuffer[i - nextPacketHeaderBytesRead];
                    }

                    CurrentPacketLength = BitConverter.ToInt32(nextPacketHeaderBytes, 0);
                    headerOffset = HEADER_BYTE_SIZE - nextPacketHeaderBytesRead;
                }
                else
                {
                    CurrentPacketLength = BitConverter.ToInt32(ReceivingBuffer, 0);
                    headerOffset = HEADER_BYTE_SIZE;
                }
                nextPacketHeaderBytesRead = 0;
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
                    nextPacketHeaderBytesRead = 0;

                    // Check to see if this message contains the header for the next message
                    for (int nextPacketHeaderIndex = 1; nextPacketHeaderIndex <= HEADER_BYTE_SIZE; nextPacketHeaderIndex++)
                    {
                        if ((nextPacketHeaderIndex + i) < receivedDataLength)
                        {
                            nextPacketHeaderBytes[nextPacketHeaderIndex - 1] = ReceivingBuffer[i + nextPacketHeaderIndex];
                            nextPacketHeaderBytesRead++;
                        }
                    }

                    i += nextPacketHeaderBytesRead;

                    if (nextPacketHeaderBytesRead == HEADER_BYTE_SIZE)
                    {
                        CurrentPacketLength = BitConverter.ToInt32(nextPacketHeaderBytes, 0);
                        nextPacketHeaderBytesRead = 0;
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
