﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NitroxModel.DataStructures.Surrogates;
using NitroxModel.Logger;
using NitroxModel.DataStructures.Util;
using NitroxModel.DataStructures.GameLogic;
using Lidgren.Network;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class Packet
    {
        private static readonly SurrogateSelector surrogateSelector;
        private static readonly StreamingContext streamingContext;
        public static readonly BinaryFormatter Serializer;
        
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

        public NetDeliveryMethod DeliveryMethod { get; protected set; } = NetDeliveryMethod.ReliableOrdered;
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
            {
                Serializer.Serialize(ms, this);
                packetData = ms.ToArray();
            }

            return packetData;
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
