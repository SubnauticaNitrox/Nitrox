using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.Communication.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxTest.Model
{
    [TestClass]
    public class PacketProcessorTest
    {
        [TestMethod]
        public void ClientPacketProcessorSanity()
        {
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
                    int numCtors = processor.GetConstructors().Length;
                    Assert.IsTrue(numCtors == 1, $"{processor} should have exactly 1 constructor! (has {numCtors})");
                });
        }

        [TestMethod]
        public void RuntimeDetectsAllClientPacketProcessors()
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();
            ClientAutoFaqRegistrar clientDependencyRegistrar = new ClientAutoFaqRegistrar();
            clientDependencyRegistrar.RegisterDependencies(containerBuilder);
            IContainer clientDependencyContainer = containerBuilder.Build(ContainerBuildOptions.IgnoreStartableComponents);

            // Check if every PacketProcessor has been detected:
            typeof(Multiplayer).Assembly.GetTypes()
                .Where(p => typeof(PacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .ToList()
                .ForEach(processor =>
                    Assert.IsTrue(clientDependencyContainer.Resolve(processor.BaseType) != null,
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
                    Type baseGenericType = processor.BaseType.GetGenericTypeDefinition();
                    Assert.IsTrue(baseGenericType == typeof(AuthenticatedPacketProcessor<>) || baseGenericType == typeof(UnauthenticatedPacketProcessor<>), $"{processor} does not derive from (Un)AuthenticatedPacketProcessor!");

                    // Check constructor availability:
                    int numCtors = processor.GetConstructors().Length;
                    Assert.IsTrue(numCtors == 1, $"{processor} should have exactly 1 constructor! (has {numCtors})");

                    // Unable to check parameters, these are defined in PacketHandler.ctor
                });
        }

        [TestMethod]
        public void SameAmountOfServerPacketProcessors()
        {
            IEnumerable<Type> processors = typeof(PacketHandler).Assembly.GetTypes()
                .Where(p => typeof(PacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);
            PacketHandler ph = new PacketHandler(new PlayerManager(), new TimeKeeper(), new SimulationOwnership());
            Dictionary<Type, PacketProcessor> authenticatedPacketProcessorsByType = (Dictionary<Type, PacketProcessor>)typeof(PacketHandler).GetField("authenticatedPacketProcessorsByType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ph);
            Dictionary<Type, PacketProcessor> unauthenticatedPacketProcessorsByType = (Dictionary<Type, PacketProcessor>)typeof(PacketHandler).GetField("unauthenticatedPacketProcessorsByType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ph);
            int both = authenticatedPacketProcessorsByType.Count + unauthenticatedPacketProcessorsByType.Count;
            Assert.AreEqual(processors.Count(), both,
                "Not all(Un) AuthenticatedPacketProcessors have been discovered by the runtime code " +
                $"(auth: {authenticatedPacketProcessorsByType.Count} + unauth: {unauthenticatedPacketProcessorsByType.Count} = {both} out of {processors.Count()}). " +
                "Perhaps the runtime matching code is too strict, or a processor does not derive from ClientPacketProcessor " +
                "(and will hence not be detected).");
        }

        [TestMethod]
        public void RuntimeDetectsAllServerPacketProcessors()
        {
            PacketHandler ph = new PacketHandler(new PlayerManager(), new TimeKeeper(), new SimulationOwnership());

            Dictionary<Type, PacketProcessor> authenticatedPacketProcessorsByType = (Dictionary<Type, PacketProcessor>)typeof(PacketHandler).GetField("authenticatedPacketProcessorsByType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ph);
            Dictionary<Type, PacketProcessor> unauthenticatedPacketProcessorsByType = (Dictionary<Type, PacketProcessor>)typeof(PacketHandler).GetField("unauthenticatedPacketProcessorsByType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ph);

            HashSet<Type> runtimeProcessors = new HashSet<Type>(authenticatedPacketProcessorsByType.Concat(unauthenticatedPacketProcessorsByType).Select(p => p.Value.GetType()));

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
            PacketHandler ph = new PacketHandler(new PlayerManager(), new TimeKeeper(), new SimulationOwnership());

            Dictionary<Type, PacketProcessor> authenticatedPacketProcessorsByType = (Dictionary<Type, PacketProcessor>)typeof(PacketHandler).GetField("authenticatedPacketProcessorsByType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ph);
            Dictionary<Type, PacketProcessor> unauthenticatedPacketProcessorsByType = (Dictionary<Type, PacketProcessor>)typeof(PacketHandler).GetField("unauthenticatedPacketProcessorsByType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ph);

            HashSet<Type> packetTypes = new HashSet<Type>(
                authenticatedPacketProcessorsByType
                    .Concat(unauthenticatedPacketProcessorsByType)
                    .Select(kvp => kvp.Key));

            ContainerBuilder containerBuilder = new ContainerBuilder();
            ClientAutoFaqRegistrar clientDependencyRegistrar = new ClientAutoFaqRegistrar();
            clientDependencyRegistrar.RegisterDependencies(containerBuilder);
            IContainer clientDependencyContainer = containerBuilder.Build(ContainerBuildOptions.IgnoreStartableComponents);

            typeof(Packet).Assembly.GetTypes()
                .Where(p => typeof(Packet).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .ToList()
                .ForEach(packet =>
                    {
                        Type clientPacketProcessorType = typeof(ClientPacketProcessor<>);
                        Type clientProcessorType = clientPacketProcessorType.MakeGenericType(packet);

                        Console.WriteLine("Checking handler for packet {0}...", packet);
                        Assert.IsTrue(packetTypes.Contains(packet) || clientDependencyContainer.Resolve(clientProcessorType) != null,
                            $"Runtime has not detected a handler for {packet}!");
                    }
                );
        }
    }
}
