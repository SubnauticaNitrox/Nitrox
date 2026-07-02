using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Nitrox.Model.Constants;
using Nitrox.Model.Core;

namespace Nitrox.Model.Helper;

/// <summary>
///     Provides access to all directories that can be pre-configured by the user of the operating system.
///     <br /><br />
///     For paths that are discovered at runtime (like launcher or game path), use <see cref="NitroxUser" />
/// </summary>
public static class NitroxDirectory
{
    private static readonly NitroxDirectoryShared implementation;

    public static string HomePath => implementation.HomePath;

    /// <inheritdoc cref="NitroxDirectoryShared.ConfigPath" />
    public static string ConfigPath => implementation.ConfigPath;

    public static string CrashLogsPath => implementation.CrashLogsPath;
    public static string ScreenshotsPath => implementation.ScreenshotsPath;
    public static string CachePath => implementation.CachePath;
    public static string SavesPath => implementation.SavesPath;
    public static string LogsPath => implementation.LogsPath;

    /// <summary>
    ///     Gets the path to Nitrox application backups. Note: not server save backups.
    /// </summary>
    public static string BackupsPath => implementation.BackupsPath;

    static NitroxDirectory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || NitroxEnvironment.IsWine)
        {
            implementation = new NitroxDirectoryUnix();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            implementation = new NitroxDirectoryMacOS();
        }
        else
        {
            implementation = new NitroxDirectoryShared();
        }
    }

    private class NitroxDirectoryShared
    {
        /// <summary>
        ///     Gets the user home directory of the current operating system user.
        /// </summary>
        public virtual string HomePath
        {
            get
            {
                if (field != null)
                {
                    return field;
                }

                // Environment variable is checked first in case user wants to override for the current process.
                string homePath = Environment.GetEnvironmentVariable("HOME");
                if (!Directory.Exists(homePath) || !Path.IsPathRooted(homePath))
                {
                    homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                }
                if (!Directory.Exists(homePath))
                {
                    throw new DirectoryNotFoundException("User home directory does not exist or is inaccessible");
                }
                if (string.IsNullOrWhiteSpace(homePath))
                {
                    throw new InvalidOperationException("User home directory is not given by the operating system");
                }
                return field = homePath;
            }
        }

        /// <summary>
        ///     User specified data path by giving --data-path argument to the running executable.
        /// </summary>
        protected string? UserSpecifiedDataPath
        {
            get
            {
                // Empty string is special value to indicate no --data-path is set. Always return null.
                if (field is "")
                {
                    return null;
                }
                // ... otherwise if value is not null it can only be a valid path.
                if (field is not null)
                {
                    return field;
                }

                // If user specified a path, use it as is.
                string? applicationData = NitroxEnvironment.CommandLineArgs.GetCommandArgs("--data-path").FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(applicationData) && Path.IsPathRooted(applicationData))
                {
                    Directory.CreateDirectory(applicationData);
                    return field = applicationData;
                }
                field = "";
                return null;
            }
        }

        /// <summary>
        ///     Gets the path where configuration files should be stored. This is %APPDATA%\Nitrox on Windows and ~/.config/Nitrox
        ///     on Linux.
        /// </summary>
        public virtual string ConfigPath
        {
            get
            {
                if (field != null)
                {
                    return field;
                }

                if (UserSpecifiedDataPath is { } userPath)
                {
                    Directory.CreateDirectory(userPath);
                    return field = userPath;
                }

                string? applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                // Finalize path to end with "Nitrox".
                applicationData = GetAsNitroxPath(applicationData);
                Directory.CreateDirectory(applicationData);
                return field = applicationData;
            }
        }

        public virtual string CrashLogsPath => Path.Combine(ConfigPath, "crashes");
        public virtual string ScreenshotsPath => Path.Combine(ConfigPath, "screenshots");
        public virtual string CachePath => Path.Combine(ConfigPath, "cache");
        public virtual string SavesPath => Path.Combine(ConfigPath, "saves");
        public virtual string LogsPath => Path.Combine(ConfigPath, "logs");
        public virtual string BackupsPath => Path.Combine(ConfigPath, "backups");

        protected string GetAsNitroxPath(string path)
        {
            if (Path.GetFileName(path) != "Nitrox")
            {
                path = Path.Combine(path, "Nitrox");
            }
            return path;
        }
    }

    /// <summary>
    ///     Special handling on Linux to follow the
    ///     <a href="https://specifications.freedesktop.org/basedir/latest/">XDG spec</a>, unless <i>--data-path</i> is given
    ///     by the user.
    /// </summary>
    private class NitroxDirectoryUnix : NitroxDirectoryShared
    {
        public override string HomePath
        {
            get
            {
                if (field != null)
                {
                    return field;
                }

                if (NitroxEnvironment.IsWine)
                {
                    string? result = Environment.GetEnvironmentVariable(NitroxConstants.HOST_HOME_ENV_VAR_NAME);
                    result ??= NitroxEnvironment.CommandLineArgs.GetCommandArgs($"--{NitroxConstants.HOST_HOME_ENV_VAR_NAME.ToLower().Replace('_', '-')}").FirstOrDefault();
                    result = result?.Replace('/', '\\');
                    if (result is { Length: >= 2 })
                    {
                        if (char.IsLetter(result[0]) && result[1] == ':')
                        {
                            result = $"Z{result[1..]}";
                        }
                        else
                        {
                            result = $"Z:{result}";
                        }
                    }
                    if (Directory.Exists(result) && Path.IsPathRooted(result))
                    {
                        return field = result;
                    }
                }

                return field = base.HomePath;
            }
    }

        public override string ConfigPath
        {
            get
            {
                if (field != null)
                {
                    return field;
                }
                return field = GetXdgPathIfNotWineOrUserGiven(XdgDirectory.CONFIG);
            }
        }

        public override string CachePath
        {
            get
            {
                if (field != null)
                {
                    return field;
                }
                return field = GetXdgPathIfNotWineOrUserGiven(XdgDirectory.CACHE);
            }
        }

        public override string ScreenshotsPath
        {
            get
            {
                if (field != null)
                {
                    return field;
                }
                return field = GetXdgPathIfNotWineOrUserGiven(XdgDirectory.DATA, "screenshots");
            }
        }

        public override string CrashLogsPath
        {
            get
            {
                if (field != null)
                {
                    return field;
                }
                return field = GetXdgPathIfNotWineOrUserGiven(XdgDirectory.STATE, "crashes");
            }
        }

        public override string LogsPath
        {
            get
            {
                if (field != null)
                {
                    return field;
                }
                return field = GetXdgPathIfNotWineOrUserGiven(XdgDirectory.STATE, "logs");
            }
        }

        public override string SavesPath
        {
            get
            {
                if (field != null)
                {
                    return field;
                }
                return field = GetXdgPathIfNotWineOrUserGiven(XdgDirectory.DATA, "saves");
            }
        }

        public override string BackupsPath
        {
            get
            {
                if (field != null)
                {
                    return field;
                }
                return field = GetXdgPathIfNotWineOrUserGiven(XdgDirectory.DATA, "backups");
            }
        }

        protected virtual string GetXdgDefaultPath(XdgDirectory directory) =>
            directory switch
            {
                XdgDirectory.DATA => Path.Combine(HomePath, ".local", "share"),
                XdgDirectory.CONFIG => Path.Combine(HomePath, ".config"),
                XdgDirectory.STATE => Path.Combine(HomePath, ".local", "state"),
                XdgDirectory.CACHE => Path.Combine(HomePath, ".cache"),
                _ => NitroxDirectory.ConfigPath
            };

        private string GetXdgPathIfNotWineOrUserGiven(XdgDirectory directory, params string[] folderNames)
        {
            if (UserSpecifiedDataPath is { } userPath)
            {
                userPath = Path.Combine([userPath, ..folderNames]);
                Directory.CreateDirectory(userPath);
                return userPath;
            }

            string? xdgPath = Environment.GetEnvironmentVariable(GetXdgName(directory));
            if (!string.IsNullOrWhiteSpace(xdgPath) && Path.IsPathRooted(xdgPath))
            {
                return xdgPath;
            }
            xdgPath = GetXdgDefaultPath(directory);
            xdgPath = Path.Combine([GetAsNitroxPath(xdgPath), ..folderNames]);
            Directory.CreateDirectory(xdgPath);
            return xdgPath;

            string GetXdgName(XdgDirectory value) =>
                value switch
                {
                    XdgDirectory.DATA => "XDG_DATA_HOME",
                    XdgDirectory.CONFIG => "XDG_CONFIG_HOME",
                    XdgDirectory.CACHE => "XDG_CACHE_HOME",
                    XdgDirectory.STATE => "XDG_STATE_HOME",
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                };
        }

        protected enum XdgDirectory
        {
            DATA,
            CONFIG,
            STATE,
            CACHE,
        }
    }

    private class NitroxDirectoryMacOS : NitroxDirectoryUnix
    {
        protected override string GetXdgDefaultPath(XdgDirectory directory) =>
            directory switch
            {
                XdgDirectory.DATA => Path.Combine(HomePath, "Library", "Application Support"),
                XdgDirectory.CONFIG => Path.Combine(HomePath, "Library", "Application Support"),
                XdgDirectory.STATE => Path.Combine(HomePath, "Library", "Application Support"),
                XdgDirectory.CACHE => Path.Combine(HomePath, "Library", "Caches"),
                _ => NitroxDirectory.ConfigPath
            };
    }
}
