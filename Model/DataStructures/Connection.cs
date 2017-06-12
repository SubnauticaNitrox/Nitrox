using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace NitroxModel.DataStructures
{
    public class Connection
    {
        public const int MessagePieceBufferSize = 4096;
        public const int MessageBufferSize = Int16.MaxValue;

        public String PlayerId { get; set; }
        public Socket Socket { get; set; }
        public byte[] MessagePieceBuffer { get; set; }
        private byte[] MessageBuffer { get; set; }

        private int MessageBufferPointer = 0;
        private Int16 NextPacketLength = 0;
        
        private IFormatter formatter = new BinaryFormatter();

        public Queue<Packet> ReceivedPackets { get; protected set; }

        public Connection()
        {
            this.ReceivedPackets = new Queue<Packet>();
            this.MessagePieceBuffer = new byte[MessagePieceBufferSize];
            this.MessageBuffer = new byte[MessageBufferSize];
        }

        public void ProcessNewMessagePiecesInBuffer(int length)
        {
            int headerOffset = 0;
            
            if(NextPacketLength == 0)
            {
                if(length < 2)
                {
                    throw new Exception("First piece of message must contain a 2 byte(Int16) message size");
                }

                NextPacketLength = BitConverter.ToInt16(MessagePieceBuffer, 0);
                headerOffset = 2;
            }
            
            for(int i = headerOffset; i < length; i++)
            {
                MessageBuffer[MessageBufferPointer] = MessagePieceBuffer[i];
                MessageBufferPointer++;
                
                if (NextPacketLength == MessageBufferPointer)
                {
                    //deserialize this packet and add it to our queue of received packets
                    using (Stream stream = new MemoryStream(MessageBuffer.Take(NextPacketLength).ToArray()))
                    {
                        ReceivedPackets.Enqueue((Packet)formatter.Deserialize(stream));
                    }

                    NextPacketLength = 0;
                    MessageBufferPointer = 0;

                    // Check to see if this message contains the header for the next message
                    if ((i + 2) < length)
                    {
                        //if so, set it and make i skip it.
                        NextPacketLength = BitConverter.ToInt16(MessagePieceBuffer, i + 1);
                        i += 2;
                    }
                }
            }
        }
        
    }
}
