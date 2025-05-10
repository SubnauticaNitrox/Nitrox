using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NitroxModel.Networking.Packets.Processors.Core;
#if NET5_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace NitroxModel.Networking.Packets.Core;

public sealed class PacketProcessorsInvoker
{
#if NET5_0_OR_GREATER
    private readonly FrozenDictionary<Type, Entry> packetTypeToProcessorEntry;
#else
    private readonly Dictionary<Type, Entry> packetTypeToProcessorEntry;
#endif

    public PacketProcessorsInvoker(IEnumerable<IPacketProcessor> packetProcessors)
    {
        if (packetProcessors is null)
        {
            throw new ArgumentOutOfRangeException(nameof(packetProcessors));
        }

        // TODO: Allow processors to handle multiple packet types (i.e. generate multiple "Entry" objects per processor type)
        Dictionary<Type, Entry> lookup = packetProcessors.Select(pInstance =>
                                                         {
                                                             return pInstance.GetType()
                                                                             .GetInterfaces()
                                                                             .Where(i => typeof(IPacketProcessor).IsAssignableFrom(i))
                                                                             .Select(i => new
                                                                             {
                                                                                 Type = i,
                                                                                 PacketType = i.GetGenericArguments().FirstOrDefault(t => typeof(Packet).IsAssignableFrom(t))
                                                                             })
                                                                             .Where(p => p.PacketType != null)
                                                                             .Select(p => new Entry(pInstance, p.Type, p.PacketType))
                                                                             .First();
                                                         })
                                                         .ToDictionary(entry => entry.PacketType, entry => entry);
#if NET5_0_OR_GREATER
        packetTypeToProcessorEntry = lookup.ToFrozenDictionary();
#else
        packetTypeToProcessorEntry = lookup;
#endif
    }

    public Entry GetProcessor(Packet packet)
    {
        Type packetType = packet.GetType();
        if (packetTypeToProcessorEntry.TryGetValue(packetType, out Entry processor))
        {
            return processor;
        }

        return null;
    }

    public sealed class Entry
    {
        private static readonly Type[] expectedProcessorParameterTypes = [typeof(IPacketProcessContext), typeof(Packet)];
        private readonly Func<IPacketProcessContext, Packet, Task> invoker;

        public Type PacketType { get; }

        internal Entry(IPacketProcessor processor, Type processorInterfaceType, Type packetType)
        {
            PacketType = packetType;

            MethodInfo method = processor.GetType().GetMethods().FirstOrDefault(m =>
            {
                if (!typeof(Task).IsAssignableFrom(m.ReturnType))
                {
                    return false;
                }
                ParameterInfo[] parameterInfos = m.GetParameters();
                if (parameterInfos.Length != expectedProcessorParameterTypes.Length)
                {
                    return false;
                }
                for (int i = 0; i < parameterInfos.Length; i++)
                {
                    if (!expectedProcessorParameterTypes[i].IsAssignableFrom(parameterInfos[i].ParameterType))
                    {
                        return false;
                    }
                }

                return true;
            });
            if (method == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(processor), $"Processor {processor.GetType()} implementing {processorInterfaceType} does not have a method that looks like 'Task M({string.Join(", ", expectedProcessorParameterTypes.Select(t => t.Name))})'");
            }
            ParameterInfo[] parameters = method.GetParameters();
            Type funcType = typeof(Func<,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, method.ReturnType);
            invoker = Unsafe.As<Func<IPacketProcessContext, Packet, Task>>(method.CreateDelegate(funcType, processor));
        }

        public Task Execute(IPacketProcessContext context, Packet packet) => invoker(context, packet);
    }
}
