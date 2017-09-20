using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Reloader based on http://github.com/pardeike/Reloader
// Modified to remove RimWorld dependency, and cleanups.

namespace NitroxReloader
{
    public class Reloader
    {
        private readonly HashSet<string> assemblyWhitelist;

        private readonly Dictionary<string, ReloaderAssembly> reloadableAssemblies;

        public Reloader(params string[] whitelist)
        {
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
                try
                {
                    var fn = Path.GetFileName(e.Name);
                    ReloaderAssembly ra;
                    if (reloadableAssemblies.TryGetValue(fn, out ra))
                    {
                        ra.Reload(e.FullPath);
                    }
                }
                catch (Exception exc)
                {
                    Console.WriteLine("An exception occured during reload!\n{0}", exc);
                }
            };
            watcher.Created += handler;
            watcher.Changed += handler;
            watcher.EnableRaisingEvents = true;
            Console.WriteLine("Reloader: Set up to watch " + ReloaderSettings.Path);
        }
    }
}
