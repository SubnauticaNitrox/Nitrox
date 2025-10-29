using System;
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
                FileInfo configFile = new(Path.GetFileName(filePath));
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
    ///     This function creates a symbolic link for the first parent JSON file. A symbolic link is required because change detection does not work with
    ///     parent files.
    /// </remarks>
    public static IConfigurationBuilder AddUpstreamJsonFile(this IConfigurationBuilder builder, string fileName, bool optional = false, bool reloadOnChange = false, bool skip = false)
    {
        if (skip)
        {
            return builder;
        }

        if (reloadOnChange)
        {
            try
            {
                // Symbolic link the first parent JSON file found. Required for change detection when file is in a parent directory.
                string current = AppContext.BaseDirectory.TrimEnd('/', '\\');
                while ((current = Path.GetDirectoryName(current)) is not null)
                {
                    string parentAppSettingsFile = Path.Combine(current, fileName);
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

        // On Linux, polling is needed to detect file changes.
        builder.AddJsonFile(new PhysicalFileProvider(AppContext.BaseDirectory)
        {
            UseActivePolling = OperatingSystem.IsLinux(), // TODO: VERIFY THIS WORKS ON WINDOWS
            UsePollingFileWatcher = OperatingSystem.IsLinux()
        }, fileName, optional, reloadOnChange);

        return builder;
    }
}
