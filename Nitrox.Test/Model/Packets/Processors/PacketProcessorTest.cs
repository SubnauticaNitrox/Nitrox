using Nitrox.Model.Core;
using Nitrox.Model.Packets.Core;
using Nitrox.Model.Platforms.Discovery;
using Nitrox.Server.Subnautica;
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
            typeof(ClientAutoFacRegistrar).Assembly.GetTypes()
                                            .Where(p => typeof(IPacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                                            .ToList()
                                            .ForEach(processor =>
                                            {
                                                Assert.IsTrue(processor.IsAssignableTo(typeof(IClientPacketProcessor)), $"{processor} does not implement {nameof(IClientPacketProcessor<>)}!");

                                                // Check constructor availability:
                                                int numCtors = processor.GetConstructors().Length;
                                                Assert.IsTrue(numCtors == 1, $"{processor} should have exactly 1 constructor! (has {numCtors})");
                                            });
        }

        [TestMethod]
        public void ServerPacketProcessorSanity()
        {
            typeof(Program).Assembly.GetTypes()
                                           .Where(p => typeof(IPacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                                           .ToList()
                                           .ForEach(processor =>
                                           {
                                               Assert.IsTrue(processor.IsAssignableTo(typeof(IAnonPacketProcessor)) || processor.IsAssignableTo(typeof(IAuthPacketProcessor)),
                                                                                           $"{processor} does not implement from any server-sided packet processor interface!");

                                               // Check constructor availability:
                                               int numCtors = processor.GetConstructors().Length;
                                               Assert.IsTrue(numCtors == 1, $"{processor} should have exactly 1 constructor! (has {numCtors})");

                                               // Unable to check parameters, these are defined in PacketHandler.ctor
                                           });
        }

        [TestMethod]
        public void AllPacketsAreHandled()
        {
            List<Type> packetTypes = typeof(GameInfo).Assembly.GetTypes().ToList();
            packetTypes.AddRange(typeof(DefaultServerPacketProcessor).Assembly.GetTypes());
            packetTypes = packetTypes.Where(p => typeof(Packet).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
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
