using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace Nitrox.Bootloader
{
    public static class Main
    {
        private static readonly Lazy<string> nitroxLauncherDir = new(() =>
        {
            // Get path from command args.
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals("-nitrox", StringComparison.OrdinalIgnoreCase) && Directory.Exists(args[i + 1]))
                {
                    return Path.GetFullPath(args[i + 1]);
                }
            }

            // Get path from environment variable.
            string envPath = Environment.GetEnvironmentVariable("NITROX_LAUNCHER_PATH");
            if (Directory.Exists(envPath))
            {
                return envPath;
            }

            // Get path from windows registry.
            using RegistryKey nitroxRegKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Nitrox");
            if (nitroxRegKey == null)
            {
                return null;
            }

            string path = nitroxRegKey.GetValue("LauncherPath") as string;
            return Directory.Exists(path) ? path : null;
        });

        public static void Execute()
        {
            string error = ValidateNitroxSetup();
            if (error != null)
            {
                Console.WriteLine(error);
                return;
            }

            Environment.SetEnvironmentVariable("NITROX_LAUNCHER_PATH", nitroxLauncherDir.Value);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomainOnAssemblyResolve;

            BootstrapNitrox();
        }

        private static void BootstrapNitrox()
        {
            Assembly core = Assembly.Load(new AssemblyName("NitroxPatcher"));
            Type mainType = core.GetType("NitroxPatcher.Main");
            mainType.InvokeMember("Execute", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, null);
        }

        private static string ValidateNitroxSetup()
        {
            if (nitroxLauncherDir.Value == null)
            {
                return "Nitrox will not load because launcher path was not provided.";
            }

            return null;
        }

        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllFileName = args.Name.Split(',')[0];
            if (!dllFileName.EndsWith(".dll"))
            {
                dllFileName += ".dll";
            }

            // Load DLLs where Nitrox launcher is first, if not found, use Subnautica's DLLs.
            string dllPath = Path.Combine(nitroxLauncherDir.Value, "lib", dllFileName);
            if (!File.Exists(dllPath))
            {
                dllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dllFileName);
            }

            if (!File.Exists(dllPath))
            {
                Console.WriteLine($"Nitrox dll missing: {dllPath}");
            }
            return Assembly.LoadFile(dllPath);
        }
    }
}