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
            "NitroxReloader.dll",
        };

        // TODO: Reloadable attribute for classes? (To define all methods in there to be reloadable)
        // TODO: See if method-size is an easy thing. If so, code could be copied but is slower.
        // TODO: Figure out assembly unloading.
        // TODO: Add reloader to server as well (only thing to reload so far is NitroxModel though)

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
                Console.WriteLine("Reloader: reading assembly " + assembly.FullName);
                return new ReloaderAssembly(assembly);
            }).Where(ra => ra.HasReloadableMethods)
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
                var fn = Path.GetFileName(e.Name);
                ReloaderAssembly ra;
                if (reloadableAssemblies.TryGetValue(fn, out ra))
                    ra.Reload(e.FullPath);
            };
            watcher.Created += handler;
            watcher.Changed += handler;
            watcher.EnableRaisingEvents = true;
            Console.WriteLine("Reloader: set up to watch " + ReloaderSettings.Path);
        }
    }
}
