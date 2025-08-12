using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Test;
using NitroxClient;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Core;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer;
using NitroxServer.Communication.Packets;
using NitroxServer.Communication.Packets.Processors;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer_Subnautica;

namespace NitroxModel.Packets.Processors
{
    [TestClass]
    public class PacketProcessorTest
    {
        [TestMethod]
        public void ClientPacketProcessorSanity()
        {
            typeof(ClientPacketProcessor<>).Assembly.GetTypes()
                               .Where(p => typeof(PacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                               .ToList()
                               .ForEach(processor =>
                               {
                                   // Make sure that each packet-processor is derived from the ClientPacketProcessor class,
                                   //  so that it's packet-type can be determined.
                                   Assert.IsNotNull(processor.BaseType, $"{processor} does not derive from any type!");
                                   Assert.IsTrue(processor.BaseType.IsGenericType, $"{processor} does not derive from a generic type!");
                                   Assert.IsTrue(processor.BaseType.IsAssignableToGenericType(typeof(ClientPacketProcessor<>)), $"{processor} does not derive from ClientPacketProcessor!");

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
                                     // Make sure that each packet-processor is derived from the ClientPacketProcessor class,
                                     //  so that it's packet-type can be determined.
                                     Assert.IsNotNull(processor.BaseType, $"{processor} does not derive from any type!");
                                     Assert.IsTrue(processor.BaseType.IsGenericType, $"{processor} does not derive from a generic type!");
                                     Assert.IsTrue(processor.BaseType.IsAssignableToGenericType(typeof(AuthenticatedPacketProcessor<>)) ||
                                                   processor.BaseType.IsAssignableToGenericType(typeof(UnauthenticatedPacketProcessor<>)), $"{processor} does not derive from (Un)AuthenticatedPacketProcessor!");

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
            Assert.AreEqual(processors.Count(), both,
                            $"Not all(Un) AuthenticatedPacketProcessors have been discovered by the runtime code (auth + unauth: {both} out of {processors.Count()}). Perhaps the runtime matching code is too strict, or a processor does not derive from ClientPacketProcessor (and will hence not be detected).");
        }

        [TestMethod]
        public void AllPacketsAreHandled()
        {
            List<Type> packetTypes = typeof(DefaultServerPacketProcessor).Assembly.GetTypes()
                                                                         .Where(p => typeof(PacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                                                                         .ToList();

            List<Type> abstractProcessorTypes = new();

            abstractProcessorTypes.AddRange(typeof(ClientPacketProcessor<>)
                                            .Assembly.GetTypes()
                                            .Where(p => p.IsClass && p.IsAbstract && p.IsAssignableToGenericType(typeof(ClientPacketProcessor<>))));

            abstractProcessorTypes.AddRange(typeof(AuthenticatedPacketProcessor<>)
                                            .Assembly.GetTypes()
                                            .Where(p => p.IsClass && p.IsAbstract && (p.IsAssignableToGenericType(typeof(AuthenticatedPacketProcessor<>)) || p.IsAssignableToGenericType(typeof(UnauthenticatedPacketProcessor<>)))));

            NitroxServiceLocator.InitializeDependencyContainer(new ClientAutoFacRegistrar(), new SubnauticaServerAutoFacRegistrar(), new TestAutoFacRegistrar());
            NitroxServiceLocator.BeginNewLifetimeScope();

            foreach (Type packet in typeof(Packet).Assembly.GetTypes().Where(p => typeof(Packet).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract).ToList())
            {
                Assert.IsTrue(packetTypes.Contains(packet) || abstractProcessorTypes.Any(genericProcessor =>
                {
                    Type processorType = genericProcessor.MakeGenericType(packet);
                    return NitroxServiceLocator.LocateOptionalService(processorType).HasValue;
                }), $"Packet of type '{packet}' should have at least one processor.");
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            NitroxServiceLocator.EndCurrentLifetimeScope();
        }
    }
}
