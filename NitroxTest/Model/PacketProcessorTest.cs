using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.Communication;
using NitroxServer.Communication.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NitroxTest.Model
{
    [TestClass]
    public class PacketProcessorTest
    {
        [TestMethod]
        public void ClientPacketProcessorSanity()
        {
            var processorParams = (Dictionary<Type, object>)typeof(Multiplayer).GetField("ProcessorArguments", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            typeof(Multiplayer).Assembly.GetTypes()
                .Where(p => typeof(PacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .ToList()
                .ForEach(processor =>
                {
                    // Make sure that each packetprocessor is derived from the ClientPacketProcessor class,
                    //  so that it's packet-type can be determined.
                    Assert.IsTrue(processor.BaseType.IsGenericType, $"{processor} does not derive from a generic type!");
                    Assert.IsTrue(processor.BaseType.GetGenericTypeDefinition() == typeof(ClientPacketProcessor<>), $"{processor} does not derive from ClientPacketProcessor!");

                    // Check constructor availability:
                    var numCtors = processor.GetConstructors().Length;
                    Assert.IsTrue(numCtors == 1, $"{processor} should have exactly 1 constructor! (has {numCtors})");

                    // Check argument type availability on constructor:
                    var ctor = processor.GetConstructors().First();
                    ctor.GetParameters().ToList().ForEach(param =>
                    {
                        Assert.IsTrue(processorParams.ContainsKey(param.ParameterType), $"Constructor for {processor} has an undefined argument of type {param.ParameterType}!");
                    });
                });
        }

        [TestMethod]
        public void SameAmountOfClientPacketProcessors()
        {
            var processors = typeof(Multiplayer).Assembly.GetTypes()
                .Where(p => typeof(PacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);
            // Note that there are less constraints; this test is mostly to ensure that someone doesn't derive from PacketProcessor but from ClientPacketProcessor (otherwise the Packet type can't be determined at runtime).
            // The RuntimeDetectsAllPacketProcessors test below shows which processors have not been detected.

            Assert.AreEqual(processors.Count(), Multiplayer.packetProcessorsByType.Count,
                "Not all ClientPacketProcessors have been discovered by the runtime code " +
                $"({Multiplayer.packetProcessorsByType.Count} out of {processors.Count()}). " +
                "Perhaps the runtime matching code is too strict, or a processor does not derive from ClientPacketProcessor " +
                "(and will hence not be detected).");
        }

        [TestMethod]
        public void RuntimeDetectsAllClientPacketProcessors()
        {
            var runtimeProcessors = new HashSet<Type>(Multiplayer.packetProcessorsByType.Select(p => p.Value.GetType()));
            // Check if every PacketProcessor has been detected:
            typeof(Multiplayer).Assembly.GetTypes()
                .Where(p => typeof(PacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .ToList()
                .ForEach(processor =>
                    Assert.IsTrue(runtimeProcessors.Contains(processor),
                        $"{processor} has not been discovered by the runtime code!")
            );
        }

        [TestMethod]
        public void ServerPacketProcessorSanity()
        {
            typeof(PacketHandler).Assembly.GetTypes()
                .Where(p => typeof(PacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .ToList()
                .ForEach(processor =>
                {
                    // Make sure that each packetprocessor is derived from the ClientPacketProcessor class,
                    //  so that it's packet-type can be determined.
                    Assert.IsTrue(processor.BaseType.IsGenericType, $"{processor} does not derive from a generic type!");
                    var baseGenericType = processor.BaseType.GetGenericTypeDefinition();
                    Assert.IsTrue(baseGenericType == typeof(AuthenticatedPacketProcessor<>) || baseGenericType == typeof(UnauthenticatedPacketProcessor<>), $"{processor} does not derive from (Un)AuthenticatedPacketProcessor!");

                    // Check constructor availability:
                    var numCtors = processor.GetConstructors().Length;
                    Assert.IsTrue(numCtors == 1, $"{processor} should have exactly 1 constructor! (has {numCtors})");

                    // Unable to check parameters, these are defined in PacketHandler.ctor
                });
        }

        [TestMethod]
        public void SameAmountOfServerPacketProcessors()
        {
            var processors = typeof(PacketHandler).Assembly.GetTypes()
                .Where(p => typeof(PacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);
            PacketHandler ph = new PacketHandler(new TcpServer(), new TimeKeeper(), new SimulationOwnership(), new GameActionManager(), new ChunkManager());
            var authenticatedPacketProcessorsByType = (Dictionary<Type, PacketProcessor>)typeof(PacketHandler).GetField("authenticatedPacketProcessorsByType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ph);
            var unauthenticatedPacketProcessorsByType = (Dictionary<Type, PacketProcessor>)typeof(PacketHandler).GetField("unauthenticatedPacketProcessorsByType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ph);
            var both = authenticatedPacketProcessorsByType.Count + unauthenticatedPacketProcessorsByType.Count;
            Assert.AreEqual(processors.Count(), both,
                "Not all(Un) AuthenticatedPacketProcessors have been discovered by the runtime code " +
                $"(auth: {authenticatedPacketProcessorsByType.Count} + unauth: {unauthenticatedPacketProcessorsByType.Count} = {both} out of {processors.Count()}). " +
                "Perhaps the runtime matching code is too strict, or a processor does not derive from ClientPacketProcessor " +
                "(and will hence not be detected).");
        }

        [TestMethod]
        public void RuntimeDetectsAllServerPacketProcessors()
        {
            PacketHandler ph = new PacketHandler(new TcpServer(), new TimeKeeper(), new SimulationOwnership(), new GameActionManager(), new ChunkManager());

            var authenticatedPacketProcessorsByType = (Dictionary<Type, PacketProcessor>)typeof(PacketHandler).GetField("authenticatedPacketProcessorsByType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ph);
            var unauthenticatedPacketProcessorsByType = (Dictionary<Type, PacketProcessor>)typeof(PacketHandler).GetField("unauthenticatedPacketProcessorsByType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ph);

            var runtimeProcessors = new HashSet<Type>(authenticatedPacketProcessorsByType.Concat(unauthenticatedPacketProcessorsByType).Select(p => p.Value.GetType()));

            // Check if every PacketProcessor has been detected:
            typeof(PacketHandler).Assembly.GetTypes()
                .Where(p => typeof(PacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .ToList()
                .ForEach(processor =>
                    Assert.IsTrue(runtimeProcessors.Contains(processor),
                        $"{processor} has not been discovered by the runtime code!")
            );
        }

        [TestMethod]
        public void AllPacketsAreHandled()
        {
            PacketHandler ph = new PacketHandler(new TcpServer(), new TimeKeeper(), new SimulationOwnership(), new GameActionManager(), new ChunkManager());

            var authenticatedPacketProcessorsByType = (Dictionary<Type, PacketProcessor>)typeof(PacketHandler).GetField("authenticatedPacketProcessorsByType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ph);
            var unauthenticatedPacketProcessorsByType = (Dictionary<Type, PacketProcessor>)typeof(PacketHandler).GetField("unauthenticatedPacketProcessorsByType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ph);

            var packetTypes = new HashSet<Type>(
                authenticatedPacketProcessorsByType
                .Concat(unauthenticatedPacketProcessorsByType)
                .Concat(Multiplayer.packetProcessorsByType)
                .Select(kvp => kvp.Key));

            typeof(Packet).Assembly.GetTypes()
                .Where(p => typeof(Packet).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .ToList()
                .ForEach(packet =>
                {
                    Console.WriteLine("Checking handler for packet {0}...", packet);
                    Assert.IsTrue(packetTypes.Contains(packet),
                        $"Runtime has not detected a handler for {packet}!");
                }
            );
        }
    }
}
