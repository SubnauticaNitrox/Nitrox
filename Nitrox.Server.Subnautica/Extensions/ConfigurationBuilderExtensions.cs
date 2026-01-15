using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Nitrox.Model.Serialization;
using Nitrox.Server.Subnautica.Models.Configuration.Providers;

namespace Nitrox.Server.Subnautica.Extensions;

internal static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddNitroxConfigFile<TOptions>(this IConfigurationBuilder configurationBuilder, string filePath, string configSectionPath = "", bool optional = false, bool reloadOnChange = false) where TOptions : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        reloadOnChange = reloadOnChange && CanChangeDetect();

        string dirPath = Path.GetDirectoryName(filePath) ?? throw new ArgumentException(nameof(filePath));
        Directory.CreateDirectory(dirPath);
        if (!File.Exists(filePath))
        {
            NitroxConfig.CreateFile<TOptions>(filePath);
        }

        if (reloadOnChange)
        {
            // Link the config to a relative path within the working directory so that IOptionsMonitor<T> works. See https://github.com/dotnet/runtime/issues/114833
            try
            {
                FileInfo configFile = new(Path.Combine(AppContext.BaseDirectory, Path.GetFileName(filePath)));
                if (configFile.Exists && configFile.LinkTarget != null)
                {
                    configFile.Delete();
                }
                configFile.CreateAsSymbolicLink(filePath);
                // Fix targets to point to symbolic link instead.
                dirPath = AppContext.BaseDirectory;
                filePath = configFile.Name; // Now a relative path.
            }
            catch (IOException)
            {
                if (!optional)
                {
                    throw;
                }
            }
        }

        PhysicalFileProvider fileProvider = new(dirPath)
        {
            UsePollingFileWatcher = true,
            UseActivePolling = true
        };
        return configurationBuilder.Add(new NitroxConfigurationSource(filePath, configSectionPath, optional, fileProvider)
        {
            ReloadOnChange = reloadOnChange,
            Optional = optional
        });
    }

    /// <summary>
    ///     Adds the first JSON file matching the file name in any parent directory of <see cref="AppContext.BaseDirectory" />.
    /// </summary>
    /// <remarks>
    ///     This function creates a symbolic link for the first parent JSON file. A symbolic link is required because change
    ///     detection does not work with
    ///     parent files.
    /// </remarks>
    public static IConfigurationBuilder AddConditionalUpstreamJsonFile(this IConfigurationBuilder builder, bool enabled, string fileName, bool optional = false, bool reloadOnChange = false)
    {
        if (!enabled)
        {
            return builder;
        }
        reloadOnChange = reloadOnChange && CanChangeDetect();

        string? parentAppSettingsFile = null;
        if (reloadOnChange)
        {
            try
            {
                // Symbolic link the first parent JSON file found. Required for change detection when file is in a parent directory.
                string current = AppContext.BaseDirectory.TrimEnd('/', '\\');
                while ((current = Path.GetDirectoryName(current)) is not null)
                {
                    parentAppSettingsFile = Path.Combine(current, fileName);
                    if (File.Exists(parentAppSettingsFile))
                    {
                        FileInfo appSettingsFile = new(Path.Combine(AppContext.BaseDirectory, fileName));
                        if (appSettingsFile.Exists && appSettingsFile.LinkTarget != null)
                        {
                            appSettingsFile.Delete();
                        }
                        appSettingsFile.CreateAsSymbolicLink(parentAppSettingsFile);
                        break;
                    }
                }
            }
            catch (IOException)
            {
                if (!optional)
                {
                    throw;
                }
            }
        }

        string? baseDirectory = CanChangeDetect() ? AppContext.BaseDirectory : Path.GetDirectoryName(parentAppSettingsFile);
        if (baseDirectory == null)
        {
            return optional ? builder : throw new Exception($"Failed to get parent directory from JSON file: {fileName}");
        }

        // On Linux, polling is needed to detect file changes.
        builder.AddJsonFile(new PhysicalFileProvider(baseDirectory)
        {
            UseActivePolling = OperatingSystem.IsLinux(),
            UsePollingFileWatcher = OperatingSystem.IsLinux()
        }, fileName, optional, reloadOnChange);

        return builder;
    }

    public static IConfigurationBuilder AddConditionalCsharpProjectJsonFile(this IConfigurationBuilder builder, bool enabled, string fileName, string? projectName = null, bool optional = false, bool reloadOnChange = false)
    {
        if (!enabled || projectName == null)
        {
            return builder;
        }

        // Symbolic link the first parent JSON file found. Required for change detection when file is in a parent directory.
        string current = AppContext.BaseDirectory.TrimEnd('/', '\\');
        string? parentAppSettingsFilePath = null;
        while ((current = Path.GetDirectoryName(current)) is not null)
        {
            if (!IsSolutionRootWithProject(current, projectName))
            {
                continue;
            }
            parentAppSettingsFilePath = Path.Combine(current, projectName, fileName);
            if (CanChangeDetect() && File.Exists(parentAppSettingsFilePath))
            {
                FileInfo appSettingsFile = new(Path.Combine(AppContext.BaseDirectory, fileName));
                if (appSettingsFile.Exists && appSettingsFile.LinkTarget != null)
                {
                    appSettingsFile.Delete();
                }
                appSettingsFile.CreateAsSymbolicLink(parentAppSettingsFilePath);
            }
            break;
        }

        string? baseDirectory = CanChangeDetect() ? AppContext.BaseDirectory : Path.GetDirectoryName(parentAppSettingsFilePath);
        if (baseDirectory == null)
        {
            return optional ? builder : throw new Exception($"Failed to get parent directory from JSON file: {fileName}");
        }

        builder.AddJsonFile(new PhysicalFileProvider(baseDirectory)
        {
            UseActivePolling = OperatingSystem.IsLinux(),
            UsePollingFileWatcher = OperatingSystem.IsLinux()
        }, fileName, optional, reloadOnChange);

        return builder;

        static bool IsSolutionRootWithProject(string path, string projectName)
        {
            bool isSolutionRoot = false;
            bool projectExists = false;
            foreach (string file in Directory.EnumerateFiles(path))
            {
                if (!isSolutionRoot && Path.GetExtension(file) is ".sln" or ".slnx")
                {
                    isSolutionRoot = true;
                }
                else if (!projectExists && Directory.Exists(Path.Combine(path, projectName)))
                {
                    projectExists = true;
                }
            }
            return isSolutionRoot && projectExists;
        }
    }

    // TODO: Handle Windows differently because symbolic link requires admin perms there. Copy file over?
    private static bool CanChangeDetect() => !OperatingSystem.IsWindows(); // For now change detection does not work on Windows.
}
