using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.DataStructures.Surrogates;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures.Surrogates;

namespace NitroxTest.Model.Packets
{
    [TestClass]
    public class PacketsSerializableTest
    {
        private readonly HashSet<Type> visitedTypes = new HashSet<Type>();

        private void IsSerializable(Type t)
        {
            if (visitedTypes.Contains(t))
            {
                return;
            }

            Assert.IsTrue(t.IsSerializable || t.IsInterface || t == typeof(SerializationInfo) || Packet.IsTypeSerializable(t), $"Type {t} is not serializable!");

            visitedTypes.Add(t);

            // Recursively check all properties and fields, because IsSerializable only checks if the current type is a primitive or has the [Serializable] attribute.
            t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).Select(tt => tt.FieldType).ForEach(IsSerializable);
        }

        [TestMethod]
        public void AllPacketsAreSerializable()
        {
            foreach (Type packetType in typeof(Packet).Assembly.GetTypes()
                .Where(p => typeof(Packet).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .ToList())
            {
                IsSerializable(packetType);
            }
        }

        [TestMethod]
        public void PacketSerializationTest()
        {
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            StreamingContext streamingContext = new StreamingContext(StreamingContextStates.All); // Our surrogates can be safely used in every context.

            Type[] types = typeof(Vector3Surrogate).Assembly
                .GetTypes();

            IEnumerable<Type> surrogates = types.Where(t =>
                                                       t.BaseType != null &&
                                                       t.BaseType.IsGenericType &&
                                                       t.BaseType.GetGenericTypeDefinition() == typeof(SerializationSurrogate<>) &&
                                                       t.IsClass &&
                                                       !t.IsAbstract);
            foreach (Type type in surrogates)
            {
                ISerializationSurrogate surrogate = (ISerializationSurrogate)Activator.CreateInstance(type);
                Type surrogatedType = type.BaseType?.GetGenericArguments()[0];
                surrogatedType.Should().NotBeNull();
                surrogateSelector.AddSurrogate(surrogatedType, streamingContext, surrogate);
            }

            // For completeness, we could pass a StreamingContextStates.CrossComputer.
            BinaryFormatter serializer = new BinaryFormatter(surrogateSelector, streamingContext);

            Stream stream = new MemoryStream();

            Type testedType = null;
            List<Packet> packets = new List<Packet>();

            foreach (Type type in typeof(Packet).Assembly.GetTypes()
            .Where(p => typeof(Packet).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract))
            {
                testedType = type;
                packets.Add((Packet)FormatterServices.GetUninitializedObject(type));
            }

            foreach (Packet packet in packets)
            {
                testedType = packet.GetType();
                serializer.Serialize(stream, packet);
                stream.Flush();
                stream.Position = 0;
                serializer.Deserialize(stream);
                stream.Position = 0;
            }
        }
    }
}
