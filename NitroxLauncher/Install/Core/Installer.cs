using System;
using System.Collections.Generic;
using System.Linq;

namespace NitroxLauncher.Install.Core;

public class Installer
{
    private readonly List<IInstaller> installers = new();

    public Installer(Func<string> subnauticaPathProvider)
    {
        installers.Add(new InstallFilePermissions(subnauticaPathProvider));
        installers.Add(new InstallUrlProtocol());
    }

    public IEnumerable<InstallResult> Install(Func<IInstaller, bool> predicate = null)
    {
        ParallelQuery<IInstaller> stream = installers.AsParallel();
        if (predicate != null)
        {
            stream = stream.Where(predicate);
        }
        return stream
               .Where(i => !i.IsInstalled)
               .Select(i =>
               {
                   try
                   {
                       return i.Install() with { Origin = i };
                   }
                   catch (Exception ex)
                   {
                       return InstallResult.From(ex) with { Origin = i };
                   }
               });
    }

    public void Uninstall()
    {
        installers.OfType<IUninstaller>()
                  .AsParallel()
                  .ForEach(installer =>
                  {
                      installer.Uninstall();
                  });
    }
}
