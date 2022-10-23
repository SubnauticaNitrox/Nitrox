using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoBogus;
using BinaryPack.Attributes;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Test.Helper.Serialization;

namespace NitroxModel.Packets
{
    [TestClass]
    public class PacketsSerializableTest
    {
        static Assembly subnauticaModelAssembly;

        [TestInitialize]
        public void Initialize()
        {
            subnauticaModelAssembly = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName("NitroxModel-Subnautica.dll"));
        }

        [TestMethod]
        public void InitSerializerTest()
        {
            Packet.InitSerializer();
        }

        [TestMethod]
        public void PacketSerializationTest()
        {
            IEnumerable<Type> types = typeof(Packet).Assembly.GetTypes().Concat(subnauticaModelAssembly.GetTypes());
            Dictionary<Type, Type[]> subtypesByBaseType = types
                .Where(type => type.IsAbstract && !type.IsSealed && !type.ContainsGenericParameters && type != typeof(Packet))
                .ToDictionary(type => type, type => types.Where(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).ToArray());

            List<Packet> packets = new();

            foreach (Type type in types.Where(p => typeof(Packet).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract))
            {
                Dictionary<Type, Type[]> subtypesDict = subtypesByBaseType.ToDictionary(pair => pair.Key, pair => pair.Value);
                object faker = typeof(NitroxAutoFaker<,>).MakeGenericType(type, typeof(PacketAutoBinder))
                    .GetConstructor(new Type[] { typeof(Dictionary<Type, Type[]>), typeof(PacketAutoBinder) })
                    .Invoke(new object[] { subtypesDict, new PacketAutoBinder(subtypesDict) });

                if (subtypesByBaseType.ContainsKey(type))
                {
                    for (int i = 0; i < subtypesByBaseType[type].Length; i++)
                    {
                        Packet packet = (Packet)typeof(AutoFaker<>).MakeGenericType(subtypesByBaseType[type][i])
                        .GetMethod(nameof(AutoFaker<object>.Generate), new[] { typeof(string) })
                        .Invoke(faker, new object[] { null });
                        packets.Add(packet);
                    }
                }
                else
                {
                    Packet packet = (Packet)typeof(AutoFaker<>).MakeGenericType(type)
                    .GetMethod(nameof(AutoFaker<object>.Generate), new[] { typeof(string) })
                    .Invoke(faker, new object[] { null });
                    packets.Add(packet);
                }
            }

            Packet.InitSerializer();
            bool failed = false;

            CompareLogic logic = new();
            logic.Config.SkipInvalidIndexers = true;
            logic.Config.AttributesToIgnore.Add(typeof(IgnoredMemberAttribute));

            foreach (Packet packet in packets)
            {
                Packet deserialized = Packet.Deserialize(packet.Serialize());

                ComparisonResult result = logic.Compare(packet, deserialized);
                if (!result.AreEqual)
                {
                    failed = true;
                    Console.WriteLine($"Differences found:\n{result.DifferencesString}");
                }
            }

            if (failed)
            {
                Assert.Fail("Error: one or more differences were found.");
            }
        }
    }
}
