namespace Nitrox.Server.Subnautica.Extensions;

internal static class TypeExtensions
{
    extension(Type self)
    {
        /// <summary>
        ///     Gets the C# source file path that defines the given type.
        /// </summary>
        public string GetCsFilePath()
        {
            string assemblyName = self.Assembly.GetName().Name ?? throw new Exception($"Failed to get assembly from type {self}");
            string nameSpaceStr = self.Namespace ?? throw new Exception($"Namespace for {self} is unknown");
            Span<char> nameSpace = stackalloc char[nameSpaceStr.Length];
            nameSpaceStr.CopyTo(nameSpace);
            nameSpace = nameSpace.Slice(assemblyName.Length + 1);
            nameSpace.Replace('.', '/');
            return $"{assemblyName}/{nameSpace}/{self.Name}.cs";
        }
    }
}
