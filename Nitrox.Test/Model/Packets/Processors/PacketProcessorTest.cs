using Nitrox.Model.Core;
using Nitrox.Model.Packets.Core;
using Nitrox.Model.Packets.Processors.Abstract;
using Nitrox.Model.Platforms.Discovery;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors;
using Nitrox.Test;
using NitroxClient;
using NitroxClient.Communication.Packets.Processors.Core;

namespace Nitrox.Model.Packets.Processors
{
    [TestClass]
    public class PacketProcessorTest
    {
        [TestMethod]
        public void ClientPacketProcessorSanity()
        {
            typeof(IClientPacketProcessor<>).Assembly.GetTypes()
                                            .Where(p => typeof(PacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                                            .ToList()
                                            .ForEach(processor =>
                                            {
                                                // Make sure that each packet-processor is derived from the ClientPacketProcessor class,
                                                //  so that it's packet-type can be determined.
                                                Assert.IsNotNull(processor.BaseType, $"{processor} does not derive from any type!");
                                                Assert.IsTrue(processor.BaseType.IsGenericType, $"{processor} does not derive from a generic type!");
                                                Assert.IsTrue(processor.BaseType.IsAssignableToGenericType(typeof(IClientPacketProcessor<>)), $"{processor} does not derive from ClientPacketProcessor!");

                                                // Check constructor availability:
                                                int numCtors = processor.GetConstructors().Length;
                                                Assert.IsTrue(numCtors == 1, $"{processor} should have exactly 1 constructor! (has {numCtors})");
                                            });
        }

        [TestMethod]
        public void ServerPacketProcessorSanity()
        {
            typeof(PacketProcessorsInvoker).Assembly.GetTypes()
                                           .Where(p => typeof(PacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                                           .ToList()
                                           .ForEach(processor =>
                                           {
                                               // Make sure that each packet-processor is derived from the ClientPacketProcessor class,
                                               //  so that it's packet-type can be determined.
                                               Assert.IsNotNull(processor.BaseType, $"{processor} does not derive from any type!");
                                               Assert.IsTrue(processor.BaseType.IsGenericType, $"{processor} does not derive from a generic type!");
                                               Assert.IsTrue(processor.BaseType.IsAssignableToGenericType(typeof(IPacketProcessor)), $"{processor} does not derive from (Un)AuthenticatedPacketProcessor!");

                                               // Check constructor availability:
                                               int numCtors = processor.GetConstructors().Length;
                                               Assert.IsTrue(numCtors == 1, $"{processor} should have exactly 1 constructor! (has {numCtors})");

                                               // Unable to check parameters, these are defined in PacketHandler.ctor
                                           });
        }

        [TestMethod]
        public void AllPacketsAreHandled()
        {
            List<Type> packetTypes = typeof(DefaultServerPacketProcessor).Assembly.GetTypes()
                                                                         .Where(p => typeof(PacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                                                                         .ToList();

            List<Type> abstractProcessorTypes = new();

            abstractProcessorTypes.AddRange(typeof(IClientPacketProcessor<>)
                                            .Assembly.GetTypes()
                                            .Where(p => p.IsClass && p.IsAbstract && p.IsAssignableToGenericType(typeof(IClientPacketProcessor<>))));

            abstractProcessorTypes.AddRange(typeof(IAuthPacketProcessor<>)
                                            .Assembly.GetTypes()
                                            .Where(p => p.IsClass && p.IsAbstract && (p.IsAssignableToGenericType(typeof(IAuthPacketProcessor<>)) || p.IsAssignableToGenericType(typeof(IAnonPacketProcessor<>)))));

            if (!GameInstallationFinder.FindGameCached(GameInfo.Subnautica))
            {
                throw new DirectoryNotFoundException("Could not find Subnautica installation.");
            }
            NitroxServiceLocator.InitializeDependencyContainer(new ClientAutoFacRegistrar(), new TestAutoFacRegistrar());
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
