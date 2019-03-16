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
using NitroxModel.Networking;
using System.Collections.Generic;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class Packet
    {
        private static readonly SurrogateSelector surrogateSelector;
        private static readonly StreamingContext streamingContext;
        private static readonly BinaryFormatter serializer;
        
        static Packet()
        {
            surrogateSelector = new SurrogateSelector();
            streamingContext = new StreamingContext(StreamingContextStates.All); // Our surrogates can be safely used in every context.

            Type[] types = Assembly.GetExecutingAssembly()
                .GetTypes();

            IEnumerable<Type> surrogates = types.Where(t =>
                                                       t.BaseType != null &&
                                                       t.BaseType.IsGenericType &&
                                                       t.BaseType.GetGenericTypeDefinition() == typeof(SerializationSurrogate<>) &&
                                                       t.IsClass &&
                                                       !t.IsAbstract);
            foreach(Type type in surrogates)
            {
                ISerializationSurrogate surrogate = (ISerializationSurrogate)Activator.CreateInstance(type);
                Type surrogatedType = type.BaseType.GetGenericArguments()[0];
                surrogateSelector.AddSurrogate(surrogatedType, streamingContext, surrogate);

                Log.Debug("Added surrogate " + surrogate + " for type " + surrogatedType);
            }

            // For completeness, we could pass a StreamingContextStates.CrossComputer.
            serializer = new BinaryFormatter(surrogateSelector, streamingContext);
        }

        public NitroxDeliveryMethod.DeliveryMethod DeliveryMethod { get; protected set; } = NitroxDeliveryMethod.DeliveryMethod.ReliableOrdered;
        public UdpChannelId UdpChannel { get; protected set; } = UdpChannelId.DEFAULT;
        public long Sequence { get; set; } = 0;

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
                serializer.Serialize(lz4Stream, this);
                packetData = ms.ToArray();
            }

            return packetData;
        }

        public static Packet Deserialize(byte[] data)
        {
            using (Stream stream = new MemoryStream(data))
            using (LZ4Stream lz4Stream = new LZ4Stream(stream, LZ4StreamMode.Decompress))
            {
                return (Packet)serializer.Deserialize(lz4Stream);
            }
        }

        public static bool IsTypeSerializable(Type type)
        {
            // We have our own surrogates to (de)serialize types that are not marked [Serializable]
            // This code is very similar to how serializability is checked in:
            // System.Runtime.Serialization.Formatters.Binary.BinaryCommon.CheckSerializable

            ISurrogateSelector selector;
            return (serializer.SurrogateSelector.GetSurrogate(type, Packet.serializer.Context, out selector) != null);
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

        public WrapperPacket ToWrapperPacket()
        {
            return new WrapperPacket(Serialize());
        }
    }
}
