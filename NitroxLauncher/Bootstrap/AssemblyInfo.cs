using System;
using System.Reflection;

namespace NitroxLauncher.Bootstrap
{
    [Serializable]
    public class AssemblyInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string RuntimeVersion { get; set; }
        public string Platform { get; set; }

        public AssemblyInfo()
        {
        }

        public AssemblyInfo(AssemblyName assemblyName)
        {
            Name = assemblyName.Name;
            Version = assemblyName.Version.ToString();
            RuntimeVersion = "";
            Platform = assemblyName.ProcessorArchitecture.ToString();
        }

        public AssemblyInfo(Assembly assembly)
            : this(assembly.GetName())
        {
            RuntimeVersion = assembly.ImageRuntimeVersion;
        }
    }
}
