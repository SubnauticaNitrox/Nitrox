using System;

namespace Nitrox.Model.Extensions;

public static class TypeExtensions
{
    extension(Type self)
    {
        public string GetCsFilePathFromType()
        {
            string assemblyName = self.Assembly.GetName().Name ?? throw new Exception($"Failed to get assembly from type {self}");
            string nameSpaceStr = self.Namespace ?? throw new Exception($"Namespace for {self} is unknown");
            if (!nameSpaceStr.StartsWith(assemblyName, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Can not get cs file path when namespace '{nameSpaceStr}' does not start with the assembly name '{assemblyName}'");
            }
#if NET
            Span<char> nameSpace = stackalloc char[nameSpaceStr.Length];
            nameSpaceStr.CopyTo(nameSpace);
            nameSpace = nameSpace.Slice(int.Clamp(assemblyName.Length + 1, 0, nameSpace.Length));
            nameSpace.Replace('.', '/');
            return $"{assemblyName}/{(nameSpace.IsEmpty ? "" : $"{nameSpace}/")}{self.Name}.cs";
#else
            nameSpaceStr = nameSpaceStr[global::Nitrox.Model.Helper.Mathf.Clamp(assemblyName.Length + 1, 0, nameSpaceStr.Length)..].Replace('.', '/');
            return $"{assemblyName}/{(nameSpaceStr.Length == 0 ? "" : $"{nameSpaceStr}/")}{self.Name}.cs";
#endif
        }
    }
}
