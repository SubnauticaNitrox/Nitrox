using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using LZ4;
using NitroxModel.DataStructures.Surrogates;
using NitroxModel.Networking;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class Packet
    {
        private static readonly SurrogateSelector surrogateSelector;
        private static readonly StreamingContext streamingContext;
        private static readonly BinaryFormatter serializer;

        private static readonly string[] blacklistedAssemblies = { "NLog" };

        private static readonly Dictionary<Type, PropertyInfo[]> cachedPropertiesByType = new();
        private static readonly StringBuilder toStringBuilder = new();

        static Packet()
        {
            surrogateSelector = new SurrogateSelector();
            streamingContext = new StreamingContext(StreamingContextStates.All); // Our surrogates can be safely used in every context.
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                                               .Where(assembly => !blacklistedAssemblies.Contains(assembly.GetName().Name))
                                               .SelectMany(a => a.GetTypes()
                                                                 .Where(t =>
                                                                            t.BaseType != null &&
                                                                            t.BaseType.IsGenericType &&
                                                                            t.BaseType.GetGenericTypeDefinition() == typeof(SerializationSurrogate<>) &&
                                                                            t.IsClass &&
                                                                            !t.IsAbstract));

            foreach (Type type in types)
            {
                ISerializationSurrogate surrogate = (ISerializationSurrogate)Activator.CreateInstance(type);
                Type surrogatedType = type.BaseType.GetGenericArguments()[0];
                surrogateSelector.AddSurrogate(surrogatedType, streamingContext, surrogate);

                Log.Debug($"Added surrogate {surrogate.GetType().Name} for type {surrogatedType}");
            }

            // For completeness, we could pass a StreamingContextStates.CrossComputer.
            serializer = new BinaryFormatter(surrogateSelector, streamingContext);
        }

        public NitroxDeliveryMethod.DeliveryMethod DeliveryMethod { get; protected set; } = NitroxDeliveryMethod.DeliveryMethod.RELIABLE_ORDERED;
        public UdpChannelId UdpChannel { get; protected set; } = UdpChannelId.DEFAULT;

        public enum UdpChannelId
        {
            DEFAULT = 0,
            PLAYER_MOVEMENT = 1,
            VEHICLE_MOVEMENT = 2,
            PLAYER_STATS = 3,
            CAMERA_MOVEMENT = 4
        }

        public byte[] Serialize()
        {
            using MemoryStream ms = new MemoryStream();
            using LZ4Stream lz4Stream = new LZ4Stream(ms, LZ4StreamMode.Compress);
            serializer.Serialize(lz4Stream, this);
            return ms.ToArray();
        }

        public static Packet Deserialize(byte[] data)
        {
            using Stream stream = new MemoryStream(data);
            using LZ4Stream lz4Stream = new LZ4Stream(stream, LZ4StreamMode.Decompress);
            return (Packet)serializer.Deserialize(lz4Stream);
        }

        public static bool IsTypeSerializable(Type type)
        {
            // We have our own surrogates to (de)serialize types that are not marked [Serializable]
            // This code is very similar to how serializability is checked in:
            // System.Runtime.Serialization.Formatters.Binary.BinaryCommon.CheckSerializable
            return serializer.SurrogateSelector.GetSurrogate(type, Packet.serializer.Context, out ISurrogateSelector _) != null;
        }

        public WrapperPacket ToWrapperPacket()
        {
            return new WrapperPacket(Serialize());
        }

        public override string ToString()
        {
            Type packetType = GetType();

            if (!cachedPropertiesByType.TryGetValue(packetType, out PropertyInfo[] properties))
            {
                properties = packetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                       .Where(x => x.Name is not nameof(DeliveryMethod) and not nameof(UdpChannel)).ToArray();
                cachedPropertiesByType.Add(packetType, properties);
            }

            toStringBuilder.Clear();
            toStringBuilder.Append($"[{packetType.Name}: ");
            foreach (PropertyInfo property in properties)
            {
                object propertyValue = property.GetValue(this);
                if (propertyValue is IList propertyList)
                {
                    toStringBuilder.Append($"{property.Name}: {propertyList.Count}, ");
                }
                else
                {
                    toStringBuilder.Append($"{property.Name}: {propertyValue}, ");
                }
            }
            toStringBuilder.Remove(toStringBuilder.Length - 2, 2);
            toStringBuilder.Append("]");

            return toStringBuilder.ToString();
        }
    }
}
