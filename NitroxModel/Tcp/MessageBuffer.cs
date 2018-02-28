﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NitroxModel.Packets;

namespace NitroxModel.Tcp
{
    public class MessageBuffer
    {
        public const int HEADER_BYTE_SIZE = 3;
        public const int RECEIVING_BUFFER_SIZE = 8192;
        public const int MESSAGE_CONSTRUCTING_BUFFER_SIZE = 1000000; // 1MB

        public byte[] ReceivingBuffer { get; set; } = new byte[RECEIVING_BUFFER_SIZE];

        private readonly byte[] messageConstructingBuffer = new byte[MESSAGE_CONSTRUCTING_BUFFER_SIZE];
        private int messageConstructingBufferPointer;
        private int currentPacketLength;

        private int nextPacketHeaderBytesRead;
        private readonly byte[] nextPacketHeaderBytes = new byte[4];

        private IFormatter formatter = Packet.Serializer;

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

                    currentPacketLength = BitConverter.ToInt32(nextPacketHeaderBytes, 0);
                    headerOffset = HEADER_BYTE_SIZE - nextPacketHeaderBytesRead;
                }
                else
                {
                    currentPacketLength = BitConverter.ToInt32(ReceivingBuffer, 0);
                    headerOffset = HEADER_BYTE_SIZE;
                }

                nextPacketHeaderBytesRead = 0;
            }

            for (int i = headerOffset; i < receivedDataLength; i++)
            {
                messageConstructingBuffer[messageConstructingBufferPointer] = ReceivingBuffer[i];
                messageConstructingBufferPointer++;

                if (currentPacketLength == messageConstructingBufferPointer)
                {
                    // deserialize this packet and add it to our queue of received packets
                    using (Stream stream = new MemoryStream(messageConstructingBuffer.Take(currentPacketLength).ToArray()))
                    {
                        yield return (Packet)formatter.Deserialize(stream);
                    }

                    currentPacketLength = 0;
                    messageConstructingBufferPointer = 0;
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
                        currentPacketLength = BitConverter.ToInt32(nextPacketHeaderBytes, 0);
                        nextPacketHeaderBytesRead = 0;
                    }
                }
            }
        }

        private bool ReadingHeaderData(int receivedDataLength)
        {
            if (currentPacketLength == 0)
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
