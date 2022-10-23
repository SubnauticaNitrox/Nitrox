using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BinaryPack.Attributes;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Test.Helper.Serialization;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[TestClass]
public class PacketsSerializableTest
{
    private static Assembly subnauticaModelAssembly;

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
        IEnumerable<Type> packetTypes = types.Where(p => typeof(Packet).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);


        // We want to ignore packets with no members when using ShouldNotCompare
        PacketAutoBinder binder = new(subtypesByBaseType);
        Type[] emptyPackets = packetTypes.Where(x => binder.GetMembers(x).Count == 0 || 
                                                     binder.GetMembers(x).All(m => m.Value.GetMemberType().IsEnum))
                                         .ToArray();

        // We generate two different versions of each packet to verify comparison is actually working
        List<Tuple<Packet, Packet>> generatedPackets = new();

        foreach (Type type in packetTypes)
        {
            NitroxAutoFakerNonGeneric faker = new(type, subtypesByBaseType, binder);

            if (subtypesByBaseType.ContainsKey(type))
            {
                for (int i = 0; i < subtypesByBaseType[type].Length; i++)
                {
                    Packet packet = faker.Generate<Packet>(subtypesByBaseType[type][i]);
                    Packet packet2 = faker.Generate<Packet>(subtypesByBaseType[type][i]);
                    generatedPackets.Add(new Tuple<Packet, Packet>(packet, packet2));
                }
            }
            else
            {
                Packet packet = faker.Generate<Packet>(type);
                Packet packet2 = faker.Generate<Packet>(type);
                generatedPackets.Add(new Tuple<Packet, Packet>(packet, packet2));
            }
        }

        Packet.InitSerializer();

        
        
        ComparisonConfig config = new();
        config.SkipInvalidIndexers = true;
        config.AttributesToIgnore.Add(typeof(IgnoredMemberAttribute));
        config.CustomComparers.Add(new CustomComparer<NitroxId, NitroxId>((id1, id2) => id1.Equals(id2)));

        foreach (Tuple<Packet, Packet> packet in generatedPackets)
        {
            Packet deserialized = Packet.Deserialize(packet.Item1.Serialize());

            packet.Item1.ShouldCompare(deserialized, $"with {packet.Item1.GetType()}", config);

            if (!emptyPackets.Contains(packet.Item1.GetType()))
            {
                packet.Item2.ShouldNotCompare(deserialized, $"with {packet.Item1.GetType()}", config);
            }
        }
    }
}
