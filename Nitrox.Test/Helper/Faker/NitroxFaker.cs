using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;

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
        { typeof(NitroxTechType), new NitroxActionFaker(typeof(NitroxTechType), f => new NitroxTechType(f.PickRandom<TechType>().ToString())) },
        { typeof(NitroxId), new NitroxActionFaker(typeof(NitroxId), f => new NitroxId(f.Random.Guid())) },
    };

    public static INitroxFaker GetOrCreateFaker(Type t)
    {
        return FakerByType.TryGetValue(t, out INitroxFaker nitroxFaker) ? nitroxFaker : CreateFaker(t);
    }

    protected static INitroxFaker CreateFaker(Type type)
    {
        if (type.IsAbstract)
        {
            return new NitroxAbstractFaker(type);
        }

        if (type.IsEnum)
        {
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
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Optional<>))
        {
            return new NitroxOptionalFaker(type);
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return new NitroxNullableFaker(type);
        }

        if (NitroxCollectionFaker.IsCollection(type, out NitroxCollectionFaker.CollectionType collectionType))
        {
            return new NitroxCollectionFaker(type, collectionType);
        }

        ConstructorInfo constructor = typeof(NitroxAutoFaker<>).MakeGenericType(type).GetConstructor(Array.Empty<Type>());

        if (constructor == null)
        {
            throw new NullReferenceException($"Could not get generic constructor for {type}");
        }

        return (INitroxFaker)constructor.Invoke(Array.Empty<object>());
    }

    protected static bool IsValidType(Type type)
    {
        return FakerByType.ContainsKey(type) ||
               type.GetCustomAttributes(typeof(DataContractAttribute), false).Length >= 1 ||
               type.GetCustomAttributes(typeof(SerializableAttribute), false).Length >= 1 ||
               (NitroxCollectionFaker.TryGetCollectionTypes(type, out Type[] collectionTypes) && collectionTypes.All(IsValidType)) ||
               type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    protected static readonly MethodInfo CastMethodBase = typeof(NitroxFaker).GetMethod(nameof(Cast), BindingFlags.NonPublic | BindingFlags.Static);

    protected static T Cast<T>(object o)
    {
        return (T)o;
    }
}
