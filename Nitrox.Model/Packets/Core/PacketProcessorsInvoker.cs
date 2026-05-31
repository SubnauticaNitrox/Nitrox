using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
#if NET
using System.Collections.Frozen;
#endif

namespace Nitrox.Model.Packets.Core;

public sealed class PacketProcessorsInvoker
{
#if NET
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

        var entries = packetProcessors.SelectMany(pInstance => pInstance.GetType()
                                                                                      .GetInterfaces()
                                                                                      .Where(i => typeof(IPacketProcessor).IsAssignableFrom(i))
                                                                                      .Select(i => new
                                                                                      {
                                                                                          InterfaceType = i,
                                                                                          PacketType = i.GetGenericArguments().FirstOrDefault(t => typeof(Packet).IsAssignableFrom(t)),
                                                                                          Processor = pInstance
                                                                                      })
                                                                                      .Where(p => p.PacketType != null));
        Dictionary<Type, Entry> lookup = [];
        foreach (var entry in entries)
        {
            if (lookup.TryGetValue(entry.PacketType, out Entry value))
            {
                if (value.Processor != entry.Processor)
                {
                    throw new Exception($"Packet type {value.PacketType} has multiple handlers (A: {value.Processor.GetType()}, B: {entry.Processor.GetType()}), which is not allowed.");
                }
                if (entry.InterfaceType.IsAssignableFrom(value.InterfaceType))
                {
                    continue;
                }
            }

            lookup[entry.PacketType] = new Entry(entry.Processor, entry.InterfaceType, entry.PacketType);
        }
#if NET
        packetTypeToProcessorEntry = lookup.ToFrozenDictionary();
#else
        packetTypeToProcessorEntry = lookup;
#endif
    }

    public Entry? GetProcessor(Type packetType)
    {
        Type current = packetType;
        Type? prior = null;
        while (current != prior)
        {
            if (packetTypeToProcessorEntry.TryGetValue(packetType, out Entry processor))
            {
                return processor;
            }
            prior = current;
            current = packetType.BaseType;
        }

        return null;
    }

    [DebuggerDisplay($"{{{nameof(InterfaceType)}}}")]
    public sealed class Entry
    {
        private static readonly Type[] expectedProcessorParameterTypes = [typeof(IPacketProcessContext), typeof(Packet)];
        private readonly Func<IPacketProcessContext, Packet, Task> invoker;

        public Type PacketType { get; }
        public Type InterfaceType { get; }

        public object Processor => invoker.Target;

        internal Entry(IPacketProcessor processor, Type processorInterfaceType, Type packetType)
        {
            PacketType = packetType;
            InterfaceType = processorInterfaceType;

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
                    Type expectedParamType = expectedProcessorParameterTypes[i];
                    // For packet parameter, we want the most specific method that can handle it.
                    if (expectedParamType == typeof(Packet))
                    {
                        expectedParamType = packetType;
                    }

                    if (!expectedParamType.IsAssignableFrom(parameterInfos[i].ParameterType))
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
            Delegate processorDelegate = method.CreateDelegate(funcType, processor);
            RuntimeHelpers.PrepareDelegate(processorDelegate);
            invoker = Unsafe.As<Func<IPacketProcessContext, Packet, Task>>(processorDelegate);
        }

        public Task Execute(IPacketProcessContext context, Packet packet) => invoker(context, packet);
        public override string ToString() => $"Processor: {invoker.Target.GetType().Name}, {nameof(InterfaceType)}: {InterfaceType.Name}";
    }
}
