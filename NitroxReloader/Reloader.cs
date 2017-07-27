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
        private static readonly HashSet<string> assemblyWhitelist = new HashSet<string>() {
            "NitroxModel.dll",
            "NitroxClient.dll",
            "NitroxPatcher.dll",
            // The reloader itself should not be allowed to reload, because it 'replaces' the ReloadableMethodAttribute
            // (as it's a different version of the assembly, that version field is saved in the other assemblies as well)
            // and suddenly all methods in the new assemblies do not refer to the ReloadableMethodAttribute found in the
            // current assembly and thus can't be found anymore (unless reloading the reloader first, then getting the
            // attribute, and then finding all methods that have that exact type as attribute).
        };

        private Dictionary<string, ReloaderAssembly> reloadableAssemblies;

        public Reloader()
        {
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
