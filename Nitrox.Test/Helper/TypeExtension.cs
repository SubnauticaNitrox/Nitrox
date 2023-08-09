using System;

namespace Nitrox.Test.Helper;

public static class TypeExtension
{
    public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
    {
        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
        {
            return true;
        }

        Type givenBaseType = givenType.BaseType;
        if (givenBaseType == null)
        {
            return false;
        }

        return IsAssignableToGenericType(givenBaseType, genericType);
    }
}
