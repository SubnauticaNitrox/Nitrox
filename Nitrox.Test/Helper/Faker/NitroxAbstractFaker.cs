using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NitroxModel_Subnautica.Logger;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer;
using NitroxServer_Subnautica;
using NitroxServer.ConsoleCommands.Abstract;

namespace Nitrox.Test.Helper.Faker;

public class NitroxAbstractFaker : NitroxFaker, INitroxFaker
{
    private static readonly Dictionary<Type, Type[]> subtypesByBaseType;

    static NitroxAbstractFaker()
    {
        Assembly[] assemblies = { typeof(Packet).Assembly, typeof(SubnauticaInGameLogger).Assembly, typeof(ServerAutoFacRegistrar).Assembly, typeof(SubnauticaServerAutoFacRegistrar).Assembly };
        HashSet<Type> blacklistedTypes = new() { typeof(Packet), typeof(CorrelatedPacket), typeof(Command), typeof(PacketProcessor) };

        List<Type> types = new();
        foreach (Assembly assembly in assemblies)
        {
            types.AddRange(assembly.GetTypes());
        }

        subtypesByBaseType = types.Where(type => type.IsAbstract && !type.IsSealed && !blacklistedTypes.Contains(type))
                                  .ToDictionary(type => type, type => types.Where(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).ToArray())
                                  .Where(dict => dict.Value.Length > 0)
                                  .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public readonly int AssignableTypesCount;
    private readonly Queue<INitroxFaker> assignableFakers = new();

    public NitroxAbstractFaker(Type type)
    {
        if (!type.IsAbstract)
        {
            throw new ArgumentException("Argument is not abstract", nameof(type));
        }

        if (!subtypesByBaseType.TryGetValue(type, out Type[] subTypes))
        {
            throw new ArgumentException($"Argument is not contained in {nameof(subtypesByBaseType)}", nameof(type));
        }

        OutputType = type;
        AssignableTypesCount = subTypes.Length;
        FakerByType.Add(type, this);
        foreach (Type subType in subTypes)
        {
            assignableFakers.Enqueue(GetOrCreateFaker(subType));
        }
    }

    public INitroxFaker[] GetSubFakers() => assignableFakers.ToArray();

    /// <summary>
    ///     Selects an implementing type in a round-robin fashion of the abstract type of this faker. Then creates an instance of it.
    /// </summary>
    public object GenerateUnsafe(HashSet<Type> typeTree)
    {
        INitroxFaker assignableFaker = assignableFakers.Dequeue();
        assignableFakers.Enqueue(assignableFaker);
        return assignableFaker.GenerateUnsafe(typeTree);
    }
}
