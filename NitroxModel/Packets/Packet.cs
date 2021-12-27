using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using LZ4;
using NitroxModel.Networking;
using ZeroFormatter;
using ZeroFormatter.Formatters;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    [DynamicUnion]
    public abstract class Packet
    {
        private static readonly Dictionary<Type, PropertyInfo[]> cachedPropertiesByType = new();
        private static readonly StringBuilder toStringBuilder = new();

        static Packet()
        {
            // TODO: setup ZeroFormatter formatters
            // Need support for:
            /*
             * BasePieceMetadata*
             * ItemData*
             * EntityMetadata*
             * RotationMetadata*
             * Version*
             * Object (SceneDebuggerChange)
             */

            Formatter.AppendDynamicUnionResolver((unionType, resolver) =>
            {
                if (unionType != typeof(Packet))
                {
                    return;
                }

                resolver.RegisterUnionKeyType(typeof(uint));

                IEnumerable<Type> GetPacketTypes()
                {
                    string[] packetAssemblies = new[] { "NitroxModel", "NitroxModel-Subnautica" };

                    return AppDomain.CurrentDomain.GetAssemblies()
                                    .Where(a => packetAssemblies.Contains(a.GetName().Name))
                                    .SelectMany(a => a.GetTypes()
                                                      .Where(t => t.IsSubclassOf(typeof(Packet))));
                }

                uint key = uint.MinValue;

                foreach (Type packetType in GetPacketTypes())
                {
                    resolver.RegisterSubType(key, packetType);
                    key++;
                }
            });
        }

        [Index(-1)]
        public NitroxDeliveryMethod.DeliveryMethod DeliveryMethod { get; protected set; } = NitroxDeliveryMethod.DeliveryMethod.RELIABLE_ORDERED;
        [Index(-2)]
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
            using MemoryStream ms = new();
            using LZ4Stream lz4Stream = new(ms, LZ4StreamMode.Compress);
            ZeroFormatterSerializer.Serialize(lz4Stream, this);
            return ms.ToArray();
        }

        public static Packet Deserialize(byte[] data)
        {
            using MemoryStream ms = new(data);
            using LZ4Stream lz4Stream = new(ms, LZ4StreamMode.Decompress);
            return ZeroFormatterSerializer.Deserialize<Packet>(lz4Stream);
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
