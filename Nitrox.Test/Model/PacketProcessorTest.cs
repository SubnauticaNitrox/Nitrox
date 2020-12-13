using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Client;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;
using Nitrox.Model.Packets.Processors.Abstract;
using Nitrox.Server;
using Nitrox.Server.Communication.Packets;
using Nitrox.Server.Communication.Packets.Processors;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica;

namespace Nitrox.Test.Model
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
            ServerAutoFacRegistrar serverDependencyRegistrar = new ServerAutoFacRegistrar();
            NitroxServiceLocator.InitializeDependencyContainer(serverDependencyRegistrar);
            NitroxServiceLocator.BeginNewLifetimeScope();

            List<Type> packetTypes = typeof(DefaultServerPacketProcessor).Assembly.GetTypes()
                .Where(p => typeof(PacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .ToList();

            int both = packetTypes.Count;
            Assert.AreEqual(processors.Count(),
                both,
                "Not all(Un) AuthenticatedPacketProcessors have been discovered by the runtime code " +
                $"(auth + unauth: {both} out of {processors.Count()}). " + // this is a small patch to keep this alive a little longer until its put out of its misery
                "Perhaps the runtime matching code is too strict, or a processor does not derive from ClientPacketProcessor " +
                "(and will hence not be detected).");
        }

        [TestMethod]
        public void AllPacketsAreHandled()
        {
            List<Type> packetTypes = typeof(DefaultServerPacketProcessor).Assembly.GetTypes()
                .Where(p => typeof(PacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .ToList();
            
            NitroxServiceLocator.InitializeDependencyContainer(new ClientAutoFacRegistrar(), new SubnauticaServerAutoFacRegistrar());
            NitroxServiceLocator.BeginNewLifetimeScope();

            foreach (Type packet in typeof(Packet).Assembly.GetTypes()
                .Where(p => typeof(Packet).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .ToList())
            {
                Type clientPacketProcessorType = typeof(ClientPacketProcessor<>);
                Type authenticatedPacketProcessorType = typeof(AuthenticatedPacketProcessor<>);
                Type unauthenticatedPacketProcessorType = typeof(UnauthenticatedPacketProcessor<>);

                Type clientProcessorType = clientPacketProcessorType.MakeGenericType(packet);
                Type authProcessorType = authenticatedPacketProcessorType.MakeGenericType(packet);
                Type unauthProcessorType = unauthenticatedPacketProcessorType.MakeGenericType(packet);

                Console.WriteLine($@"Checking handler for packet {packet}...");
                (packetTypes.Contains(packet) ||
                 NitroxServiceLocator.LocateOptionalService(clientProcessorType).HasValue ||
                 NitroxServiceLocator.LocateOptionalService(authProcessorType).HasValue ||
                 NitroxServiceLocator.LocateOptionalService(unauthProcessorType).HasValue).Should()
                    .BeTrue($"Packet of type '{packet}' should have at least one processor.");
            }
        }
    }
}
