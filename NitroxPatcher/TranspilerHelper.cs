using System;
using System.Reflection;
using System.Reflection.Emit;
using NitroxModel.Core;

namespace NitroxPatcher
{
    static class TranspilerHelper
    {
        private static readonly MethodInfo serviceLocator = typeof(NitroxServiceLocator)
            .GetMethod("LocateService", BindingFlags.Static | BindingFlags.Public, null, new Type[] { }, null);

        public static ValidatedCodeInstruction LocateService<T>()
        {
            return new ValidatedCodeInstruction(OpCodes.Call, serviceLocator.MakeGenericMethod(typeof(T)));
        }
    }
}
