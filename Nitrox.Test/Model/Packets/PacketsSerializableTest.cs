using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Packets;
using ZeroFormatter;

namespace NitroxTest.Model.Packets
{
    [TestClass]
    public class PacketsSerializableTest
    {
        private void IsSerializable(Type t)
        {
            Assert.IsTrue(t.GetCustomAttribute<ZeroFormattableAttribute>() != null || InDynamicUnion(t));
        }

        private bool InDynamicUnion(Type t)
        {
            return t.GetCustomAttribute<DynamicUnionAttribute>() != null || InDynamicUnion(t.BaseType);
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
            string[] assemblies = new[] { "NitroxModel", "NitroxModel-Subnautica" };

            Stream stream = new MemoryStream();

            Type testedType = null;
            List<Packet> packets = new List<Packet>();

            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies()
                                                         .Where(a => Enumerable.Contains(assemblies, a.GetName().Name))
                                                         .SelectMany(a => a.GetTypes()
                                                                           .Where(p => typeof(Packet).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)))
            {
                testedType = type;
                packets.Add((Packet)FormatterServices.GetUninitializedObject(type));
            }

            foreach (Packet packet in packets)
            {
                testedType = packet.GetType();
                ZeroFormatterSerializer.Serialize(stream, packet);
                stream.Flush();
                stream.Position = 0;
                ZeroFormatterSerializer.NonGeneric.Deserialize(testedType, stream);
                stream.Position = 0;
            }
        }
    }
}
