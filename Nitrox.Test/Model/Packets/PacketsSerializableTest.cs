using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BinaryPack.Attributes;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Test.Helper.Faker;
using NitroxModel_Subnautica.Logger;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[TestClass]
public class PacketsSerializableTest
{
    [TestMethod]
    public void InitSerializerTest()
    {
        Packet.InitSerializer();
    }

    [TestMethod]
    public void PacketSerializationTest()
    {
        ComparisonConfig config = new();
        config.SkipInvalidIndexers = true;
        config.AttributesToIgnore.Add(typeof(IgnoredMemberAttribute));
        config.CustomComparers.Add(new CustomComparer<NitroxId, NitroxId>((id1, id2) => id1.Equals(id2)));
        CompareLogic comparer = new(config);

        IEnumerable<Type> types = typeof(Packet).Assembly.GetTypes().Concat(typeof(SubnauticaInGameLogger).Assembly.GetTypes());
        Type[] packetTypes = types.Where(p => typeof(Packet).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract).ToArray();

        // We want to ignore packets with no members when using ShouldNotCompare
        Type[] emptyPackets = packetTypes.Where(t => !t.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                                                       .Any(member => member.MemberType is MemberTypes.Field or MemberTypes.Property &&
                                                                      !member.GetCustomAttributes<IgnoredMemberAttribute>().Any()))
                                         .ToArray();

        // We generate two different versions of each packet to verify comparison is actually working
        List<(Packet, Packet)> generatedPackets = new();

        foreach (Type type in packetTypes)
        {
            dynamic faker = NitroxFaker.GetOrCreateFaker(type);

            Packet packet = faker.Generate();
            Packet packet2 = null;

            if (!emptyPackets.Contains(type))
            {
                ComparisonResult result;
                do
                {
                    packet2 = faker.Generate();
                    result = comparer.Compare(packet, packet2);
                } while (result == null || result.AreEqual);
            }

            generatedPackets.Add(new ValueTuple<Packet, Packet>(packet, packet2));
        }

        Packet.InitSerializer();

        foreach (ValueTuple<Packet, Packet> packet in generatedPackets)
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
