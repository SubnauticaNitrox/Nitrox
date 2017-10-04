using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

// Reloader based on http://github.com/pardeike/Reloader
// Modified to remove RimWorld dependency, and cleanups.

namespace NitroxReloader
{
    public static class Reloader
    {
        private static HashSet<string> assemblyWhitelist;
        private static Dictionary<string, ReloaderAssembly> reloadableAssemblies;
        private static Queue<string> changedFiles = new Queue<string>();

        [Conditional("DEBUG")]
        public static void Initialize(params string[] whitelist)
        {
            Console.WriteLine("[NITROX] Initializing reloader...");

            assemblyWhitelist = new HashSet<string>(whitelist);

            reloadableAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly =>
            {
                string location;
                try
                {
                    // Dynamic assemblies do not have a Location and thus cause an error.
                    location = assembly.Location;
                }
                catch (NotSupportedException)
                {
                    return false;
                }
                return assemblyWhitelist.Contains(Path.GetFileName(location));
            })
            .Select(assembly =>
            {
                Console.WriteLine("Reloader: Reading assembly " + assembly.FullName);
                return new ReloaderAssembly(assembly);
            })
            .ToDictionary(ra => ra.AssemblyName, null);

            var watcher = new FileSystemWatcher()
            {
                Path = ReloaderSettings.Path,
                Filter = "*.dll",
                NotifyFilter = NotifyFilters.CreationTime
                | NotifyFilters.LastWrite
                | NotifyFilters.FileName
                | NotifyFilters.DirectoryName
            };
            FileSystemEventHandler handler = (s, e) =>
            {
                lock (changedFiles)
                {
                    if (!changedFiles.Contains(e.FullPath))
                    {
                        changedFiles.Enqueue(e.FullPath);
                    }
                }
            };
            watcher.Created += handler;
            watcher.Changed += handler;
            watcher.EnableRaisingEvents = true;
            Console.WriteLine("[NITROX] Reloader set up to watch " + ReloaderSettings.Path);
        }

        [Conditional("DEBUG")]
        public static void ReloadAssemblies()
        {
            if (changedFiles.Count == 0)
            {
                // Prevent unnecessary locking.
                return;
            }

            lock (changedFiles)
            {
                while (changedFiles.Count > 0)
                {
                    string path = changedFiles.Dequeue();
                    try
                    {
                        var fn = Path.GetFileName(path);
                        ReloaderAssembly ra;
                        if (reloadableAssemblies.TryGetValue(fn, out ra))
                        {
                            ra.Reload(path);
                        }
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("An exception occured during reload!\n{0}", exc);
                    }
                }
            }
        }
    }
}
