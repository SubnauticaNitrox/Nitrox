using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BinaryPack.Attributes;
using NitroxModel.Networking;
using BinaryConverter = BinaryPack.BinaryConverter;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class Packet
    {
        private static readonly Dictionary<Type, PropertyInfo[]> cachedPropertiesByType = new();
        private static readonly StringBuilder toStringBuilder = new();

        public static void InitSerializer()
        {
            static IEnumerable<Type> FindTypesInModelAssemblies()
            {
                return AppDomain.CurrentDomain.GetAssemblies()
                                              .Where(assembly => new string[] { "NitroxModel", "NitroxModel-Subnautica" }
                                                                 .Contains(assembly.GetName().Name))
                                              .SelectMany(assembly =>
                                              {
                                                  try
                                                  {
                                                      return assembly.GetTypes();
                                                  }
                                                  catch (ReflectionTypeLoadException e)
                                                  {
                                                      return e.Types.Where(t => t != null);
                                                  }
                                              });
            }

            static IEnumerable<Type> FindUnionBaseTypes() => FindTypesInModelAssemblies()
                .Where(t => t.IsAbstract && !t.IsSealed && (!t.BaseType?.IsAbstract ?? true) && !t.ContainsGenericParameters);

            foreach (Type type in FindUnionBaseTypes())
            {
                BinaryConverter.RegisterUnion(type, FindTypesInModelAssemblies()
                    .Where(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                    .OrderByDescending(t =>
                    {
                        Type current = t;
                        int levels = 0;

                        while (current != type && current != null)
                        {
                            current = current.BaseType;
                            levels++;
                        }

                        return levels;
                    })
                    .ThenBy(t => t.FullName)
                    .ToArray());
            }

            // This will initialize the processor for Wrapper which will initialize all the others
            _ = BinaryConverter.Serialize(new Wrapper());
        }

        [IgnoredMember]
        public NitroxDeliveryMethod.DeliveryMethod DeliveryMethod { get; protected set; } = NitroxDeliveryMethod.DeliveryMethod.RELIABLE_ORDERED;
        [IgnoredMember]
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
            return BinaryConverter.Serialize(new Wrapper(this));
        }

        public static Packet Deserialize(byte[] data)
        {
            return BinaryConverter.Deserialize<Wrapper>(data).Packet;
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

        /// <summary>
        ///     Wrapper which is used to serialize packets in BinaryPack.
        ///     We cannot serialize Packets directly because
        ///     <p>
        ///     1) We will not know what type to deserialize to and
        ///     2) The root object must have a callable constructor so it can't be abstract
        ///     </p>
        ///     This type solves both problems and only adds a single byte to the data.
        /// </summary>
        public readonly struct Wrapper
        {
            public Packet Packet { get; init; } = null;

            public Wrapper(Packet packet)
            {
                Packet = packet;
            }
        }
    }
}
