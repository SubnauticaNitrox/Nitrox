using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NitroxModel.Packets.Processors.Abstract
{
    public abstract class PacketProcessor
    {
        public abstract void ProcessPacket(Packet packet, IProcessorContext context);

        public static Dictionary<Type, PacketProcessor> GetProcessors(Dictionary<Type, object> processorArguments, Func<Type, bool> additionalConstraints)
        {
            return Assembly.GetCallingAssembly()
                .GetTypes()
                .Where(p => typeof(PacketProcessor).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .Where(additionalConstraints)
                .ToDictionary(proc => proc.BaseType.GetGenericArguments()[0], proc =>
                {
                    ConstructorInfo[] ctors = proc.GetConstructors();
                    if (ctors.Length > 1)
                    {
                        throw new NotSupportedException($"{proc.Name} has more than one constructor!");
                    }

                    ConstructorInfo ctor = ctors.First();

                    // Prepare arguments for constructor (if applicable):
                    object[] args = ctor.GetParameters().Select(pi =>
                        {
                            if (processorArguments.TryGetValue(pi.ParameterType, out object v))
                            {
                                return v;
                            }

                            throw new ArgumentException($"Argument value not defined for type {pi.ParameterType}! Used in {proc}");
                        }).ToArray();

                    return (PacketProcessor)ctor.Invoke(args);
                });
        }
    }
}
