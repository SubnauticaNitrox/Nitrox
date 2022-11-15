using System;
using System.Collections.Generic;
using System.Reflection;
using AutoBogus;
using Bogus;

namespace Nitrox.Test.Helper.Serialization;

/// <summary>
/// <see cref="NitroxAutoFaker{TType,TBinder}"/> wrapper with non-generic type support
/// </summary>
/// <inheritdoc cref="NitroxAutoFaker{TType,TBinder}"/>
public class NitroxAutoFakerNonGeneric
{
    private readonly object faker;

    public NitroxAutoFakerNonGeneric(Type type, Dictionary<Type, Type[]> subtypesByBaseType, IBinder binder)
    {
        ConstructorInfo constructor = typeof(NitroxAutoFaker<,>).MakeGenericType(type, typeof(PacketAutoBinder))
                                                                .GetConstructor(new[] { typeof(Dictionary<Type, Type[]>), typeof(PacketAutoBinder) });

        if (constructor == null)
        {
            throw new NullReferenceException($"Could not get generic constructor for {type}");
        }

        faker = constructor.Invoke(new object[] { subtypesByBaseType, binder });
    }

    public T Generate<T>(Type type)
    {
        MethodInfo method = typeof(AutoFaker<>).MakeGenericType(type)
                                               .GetMethod(nameof(AutoFaker<object>.Generate), new[] { typeof(string) });

        if (method == null)
        {
            throw new NullReferenceException($"Could not get generic method for {type}");
        }

        return (T)method.Invoke(faker, new object[] { null });
    }
}
