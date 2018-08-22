using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NitroxModel.DataStructures.Surrogates;
using NitroxModel.Logger;
using NitroxModel.DataStructures.Util;
using NitroxModel.DataStructures.GameLogic;
using LZ4;
using LiteNetLib;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class Packet
    {
        private static readonly SurrogateSelector surrogateSelector;
        private static readonly StreamingContext streamingContext;
        private static readonly BinaryFormatter Serializer;
        
        static Packet()
        {
            surrogateSelector = new SurrogateSelector();
            streamingContext = new StreamingContext(StreamingContextStates.All); // Our surrogates can be safely used in every context.

            Type[] types = Assembly.GetExecutingAssembly()
                .GetTypes();

            types.Where(t =>
                    t.BaseType != null &&
                    t.BaseType.IsGenericType &&
                    t.BaseType.GetGenericTypeDefinition() == typeof(SerializationSurrogate<>) &&
                    t.IsClass &&
                    !t.IsAbstract)
                .ForEach(t =>
                {
                    ISerializationSurrogate surrogate = (ISerializationSurrogate)Activator.CreateInstance(t);
                    Type surrogatedType = t.BaseType.GetGenericArguments()[0];
                    surrogateSelector.AddSurrogate(surrogatedType, streamingContext, surrogate);

                    Log.Debug("Added surrogate " + surrogate + " for type " + surrogatedType);
                });

            // For completeness, we could pass a StreamingContextStates.CrossComputer.
            Serializer = new BinaryFormatter(surrogateSelector, streamingContext);
        }

        public SendOptions DeliveryMethod { get; protected set; } = SendOptions.ReliableOrdered;
        public UdpChannelId UdpChannel { get; protected set; } = UdpChannelId.DEFAULT;

        public enum UdpChannelId
        {
            DEFAULT = 0,
            PLAYER_MOVEMENT = 1,
            VEHICLE_MOVEMENT = 2,
            PLAYER_STATS = 3
        }

        public byte[] Serialize()
        {
            byte[] packetData;

            using (MemoryStream ms = new MemoryStream())
            using (LZ4Stream lz4Stream = new LZ4Stream(ms, LZ4StreamMode.Compress))
            {
                Serializer.Serialize(lz4Stream, this);
                packetData = ms.ToArray();
            }

            return packetData;
        }

        public static Packet Deserialize(byte[] data)
        {
            using (Stream stream = new MemoryStream(data))
            using (LZ4Stream lz4Stream = new LZ4Stream(stream, LZ4StreamMode.Decompress))
            {
                return (Packet)Serializer.Deserialize(lz4Stream);
            }
        }

        public static bool IsTypeSerializable(Type type)
        {
            // We have our own surrogates to (de)serialize types that are not marked [Serializable]
            // This code is very similar to how serializability is checked in:
            // System.Runtime.Serialization.Formatters.Binary.BinaryCommon.CheckSerializable

            ISurrogateSelector selector;
            return (Serializer.SurrogateSelector.GetSurrogate(type, Packet.Serializer.Context, out selector) != null);
        }

        // Deferred cells are a replacement for the old DeferredPacket class.  The idea
        // is that some packets should not be replayed until a player enters close proximity.
        // when the player enters a deferred cell, the DeferredPacketReceiver will automatically
        // allow the packet to be processed. This method is virtual as some packets may have
        // complex logic to decide if it needs to defer.
        public virtual Optional<AbsoluteEntityCell> GetDeferredCell()
        {
            return Optional<AbsoluteEntityCell>.Empty();
        }
    }
}
