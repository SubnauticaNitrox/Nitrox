using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

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

            // Get path from AppData file.
            string nitroxAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nitrox");
            if (!Directory.Exists(nitroxAppData))
            {
                return null;
            }
            string nitroxLauncherPathFile = Path.Combine(nitroxAppData, "launcherpath.txt");
            if (!File.Exists(nitroxLauncherPathFile))
            {
                return null;
            }

            try
            {
                string valueInFile = File.ReadAllText(nitroxLauncherPathFile).Trim();
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        // Delete the path so that the launcher should be used to launch Nitrox
                        File.Delete(nitroxLauncherPathFile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unable to delete the launcherpath.txt file. Nitrox will launch again without launcher. Error:{Environment.NewLine}{ex}");
                    }
                });
                return Directory.Exists(valueInFile) ? valueInFile : null;
            }
            catch
            {
                // ignored
            }
            return null;
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
                return "Nitrox launcher path not set in AppData. Nitrox will not start.";
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