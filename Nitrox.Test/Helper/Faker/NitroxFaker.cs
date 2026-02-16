using System.Runtime.Serialization;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Test.Helper.Faker;

public interface INitroxFaker
{
    public Type OutputType { get; }
    public INitroxFaker[] GetSubFakers();
    public object GenerateUnsafe(HashSet<Type> typeTree);
}

public abstract class NitroxFaker
{
    public Type OutputType { get; protected init; }
    protected static readonly Bogus.Faker Faker;

    static NitroxFaker()
    {
        Faker = new Bogus.Faker();
    }

    protected static readonly Dictionary<Type, INitroxFaker> FakerByType = new()
    {
        // Basic types
        { typeof(bool), new NitroxActionFaker(typeof(bool), f => f.Random.Bool()) },
        { typeof(byte), new NitroxActionFaker(typeof(byte), f => f.Random.Byte()) },
        { typeof(sbyte), new NitroxActionFaker(typeof(sbyte), f => f.Random.SByte()) },
        { typeof(short), new NitroxActionFaker(typeof(short), f => f.Random.Short()) },
        { typeof(ushort), new NitroxActionFaker(typeof(ushort), f => f.Random.UShort()) },
        { typeof(int), new NitroxActionFaker(typeof(int), f => f.Random.Int()) },
        { typeof(uint), new NitroxActionFaker(typeof(uint), f => f.Random.UInt()) },
        { typeof(long), new NitroxActionFaker(typeof(long), f => f.Random.Long()) },
        { typeof(ulong), new NitroxActionFaker(typeof(ulong), f => f.Random.ULong()) },
        { typeof(decimal), new NitroxActionFaker(typeof(decimal), f => f.Random.Decimal()) },
        { typeof(float), new NitroxActionFaker(typeof(float), f => f.Random.Float()) },
        { typeof(double), new NitroxActionFaker(typeof(double), f => f.Random.Double()) },
        { typeof(char), new NitroxActionFaker(typeof(char), f => f.Random.Char()) },
        { typeof(string), new NitroxActionFaker(typeof(string), f => f.Random.Word()) },

        // Nitrox types
        { typeof(PeerId), new NitroxActionFaker(typeof(PeerId), f => (PeerId)f.Random.UInt() )},
        { typeof(SessionId), new NitroxActionFaker(typeof(SessionId), f => (SessionId)f.Random.UShort() )},
        { typeof(NitroxTechType), new NitroxActionFaker(typeof(NitroxTechType), f => new NitroxTechType(f.PickRandom<TechType>().ToString())) },
        { typeof(NitroxId), new NitroxActionFaker(typeof(NitroxId), f => new NitroxId(f.Random.Guid())) },
    };

    public static INitroxFaker GetOrCreateFaker(Type t)
    {
        return FakerByType.TryGetValue(t, out INitroxFaker nitroxFaker) ? nitroxFaker : CreateFaker(t);
    }

    protected static INitroxFaker CreateFaker(Type type)
    {
        switch (type)
        {
            case { IsEnum: true }:
                return new NitroxActionFaker(type, f =>
                {
                    string[] selection = Enum.GetNames(type);
                    if (selection.Length == 0)
                    {
                        throw new ArgumentException("There are no enum values after exclusion to choose from.");
                    }

                    string val = f.Random.ArrayElement(selection);
                    return Enum.Parse(type, val);
                });
            case not null when NitroxCollectionFaker.IsCollection(type, out NitroxCollectionFaker.CollectionType collectionType):
                return new NitroxCollectionFaker(type, collectionType);
            case { IsAbstract: true }:
                return new NitroxAbstractFaker(type);
            case { IsGenericType: true } when type.GetGenericTypeDefinition() is { } genericTypeDefinition:
                if (genericTypeDefinition == typeof(Optional<>))
                {
                    return new NitroxOptionalFaker(type);
                }
                if (genericTypeDefinition == typeof(Nullable<>))
                {
                    return new NitroxNullableFaker(type);
                }
                break;
        }

        ConstructorInfo constructor = typeof(NitroxAutoFaker<>).MakeGenericType(type).GetConstructor([]);
        if (constructor == null)
        {
            throw new NotSupportedException($"Type {type} does not have a compatible {nameof(INitroxFaker)} yet");
        }
        return (INitroxFaker)constructor.Invoke([]);
    }

    protected static bool IsValidType(Type type)
    {
        return FakerByType.ContainsKey(type) ||
               type.GetCustomAttributes(typeof(DataContractAttribute), false).Length >= 1 ||
               type.GetCustomAttributes(typeof(SerializableAttribute), false).Length >= 1 ||
               (NitroxCollectionFaker.TryGetCollectionTypes(type, out Type[] collectionTypes) && collectionTypes.All(IsValidType)) ||
               type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    protected static readonly MethodInfo CastMethodBase = typeof(NitroxFaker).GetMethod(nameof(Cast), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new Exception($"{nameof(NitroxFaker)} has no {nameof(Cast)} method!");

    protected static T Cast<T>(object o)
    {
        return (T)o;
    }
}
