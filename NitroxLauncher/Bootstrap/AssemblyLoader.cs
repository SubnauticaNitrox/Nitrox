using System;
using System.Reflection;

namespace NitroxLauncher.Bootstrap
{
    public class AssemblyLoader : MarshalByRefObject
    {
        public AssemblyInfo LoadAssemlyInfo(string assemblyPath)
        {
            Assembly assembly = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
            return new AssemblyInfo(assembly);
        }
    }
}
