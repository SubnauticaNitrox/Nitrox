using Nitrox.Server.Subnautica;
using NitroxClient;
using NitroxModel.Core;
using NitroxModel.Networking.Packets;
using NitroxModel.Networking.Packets.Processors.Core;

namespace NitroxModel.Packets.Processors;

[TestClass]
public class PacketProcessorTest
{
    [TestMethod]
    public void AllPacketsAreHandled()
    {
        Type[] allTypes = new []{ typeof(Program).Assembly, typeof(ClientAutoFacRegistrar).Assembly, typeof(GameInfo).Assembly }.SelectMany(a => a.GetTypes()).ToArray();
        Type[] packetTypes = allTypes.Where(p => typeof(Packet).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract).ToArray();
        Type[] processorImplementations = allTypes.Where(p =>
        {
            if (!p.IsClass || p.IsAbstract || p.IsInterface)
            {
                return false;
            }
            if (typeof(IPacketProcessor).IsAssignableFrom(p))
            {
                return true;
            }
            return false;
        }).ToArray();

        Assert.IsNotEmpty(allTypes);
        Assert.IsNotEmpty(packetTypes);
        Assert.IsNotEmpty(processorImplementations);
        foreach (Type packet in packetTypes)
        {
            Assert.IsTrue(processorImplementations.Any(p => IsProcessorForPacket(p, packet)), $"Packet of type '{packet}' should have at least one processor.");
        }

        bool IsProcessorForPacket(Type processor, Type packet)
        {
            foreach (Type face in processor.GetInterfaces())
            {
                if (!typeof(IPacketProcessor).IsAssignableFrom(face))
                {
                    continue;
                }
                foreach (Type genericArg in face.GetGenericArguments())
                {
                    if (genericArg == packet)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    [TestCleanup]
    public void Cleanup() => NitroxServiceLocator.EndCurrentLifetimeScope();
}
