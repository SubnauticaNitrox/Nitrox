using System;
using System.IO;
using System.Reflection;

namespace Nitrox.Bootloader
{
    public static class Main
    {
        public static void Execute()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomainOnAssemblyResolve;

            BootstrapNitrox();
        }
        
        private static void BootstrapNitrox()
        {
            Assembly core = Assembly.Load(new AssemblyName("NitroxPatcher"));
            Type mainType = core.GetType("NitroxPatcher.Main");
            mainType.InvokeMember("Execute", BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[0]);
        }

        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllFileName = args.Name.Split(',')[0] + ".dll";

            // Load DLLs where Nitrox launcher is first, if not found, use Subnautica's DLLs.
            string dllPath = Path.Combine("libs",dllFileName);
            // if (!File.Exists(dllPath))
            // {
            //     dllPath = Path.Combine(ManagedLibsDir, dllFileName);
            // }

            return Assembly.LoadFile(dllPath);
        }
    }
}
