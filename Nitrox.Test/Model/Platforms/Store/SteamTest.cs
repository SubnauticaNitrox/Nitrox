using System.Runtime.Versioning;
using Nitrox.Test.Model.Platforms;

namespace Nitrox.Model.Platforms.Store;

[TestClass]
[SupportedOSPlatform("linux")]
public class SteamTest
{
    private string tempRoot = null!;
    private string? originalXdgDataDirs;

    [TestInitialize]
    public void Setup()
    {
        tempRoot = Path.Combine(Path.GetTempPath(), $"NitroxSteamTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        originalXdgDataDirs = Environment.GetEnvironmentVariable("XDG_DATA_DIRS");
    }

    [TestCleanup]
    public void Cleanup()
    {
        Environment.SetEnvironmentVariable("XDG_DATA_DIRS", originalXdgDataDirs);
        Directory.Delete(tempRoot, true);
    }

    [OSTestMethod(OperatingSystems.Linux)]
    public void GetProtonPathByVersion_FindsCustomProton_InSystemWideXdgCompatToolsDir()
    {
        string steamRoot = CreateSteamRoot();
        string xdgShare = Path.Combine(tempRoot, "xdg-share");
        string systemProtonDir = Path.Combine(xdgShare, "steam", "compatibilitytools.d", "proton-cachyos-slr");
        Directory.CreateDirectory(systemProtonDir);
        File.WriteAllText(Path.Combine(systemProtonDir, "proton"), "");
        Environment.SetEnvironmentVariable("XDG_DATA_DIRS", xdgShare);

        Steam.SteamLibrariesVdf steamLibraries = Steam.SteamLibrariesVdf.Load(steamRoot);
        string? result = steamLibraries.GetProtonPathByVersion("proton-cachyos-slr");

        result.Should().Be(systemProtonDir);
    }

    [OSTestMethod(OperatingSystems.Linux)]
    public void GetProtonPathByVersion_ReturnsNull_WhenCustomProtonNotInstalledAnywhere()
    {
        string steamRoot = CreateSteamRoot();
        Environment.SetEnvironmentVariable("XDG_DATA_DIRS", Path.Combine(tempRoot, "nonexistent-xdg-share"));

        Steam.SteamLibrariesVdf steamLibraries = Steam.SteamLibrariesVdf.Load(steamRoot);
        string? result = steamLibraries.GetProtonPathByVersion("proton-cachyos-slr");

        result.Should().BeNull();
    }

    [OSTestMethod(OperatingSystems.Linux)]
    public void GetRuntimePathForProton_UsesRequireToolAppIdFromToolManifest()
    {
        const string runtimeAppId = "4183110";
        string steamRoot = CreateSteamRoot();
        string libraryPath = Path.Combine(tempRoot, "library");
        CreateSteamApp(libraryPath, runtimeAppId, "SteamLinuxRuntime_4");
        WriteLibraryFolders(steamRoot, libraryPath, runtimeAppId);

        string protonPath = Path.Combine(tempRoot, "proton-cachyos-slr");
        Directory.CreateDirectory(protonPath);
        File.WriteAllText(Path.Combine(protonPath, "toolmanifest.vdf"), $$"""
            "manifest"
            {
                "require_tool_appid" "{{runtimeAppId}}"
            }
            """);

        Steam.SteamLibrariesVdf steamLibraries = Steam.SteamLibrariesVdf.Load(steamRoot);
        string? result = steamLibraries.GetRuntimePathForProton(protonPath);

        result.Should().Be(Path.Combine(libraryPath, "steamapps", "common", "SteamLinuxRuntime_4"));
    }

    [OSTestMethod(OperatingSystems.Linux)]
    public void GetRuntimePathForProton_DefaultsToSniper_WhenProtonHasNoToolManifest()
    {
        const string sniperAppId = "1628350";
        string steamRoot = CreateSteamRoot();
        string libraryPath = Path.Combine(tempRoot, "library");
        CreateSteamApp(libraryPath, sniperAppId, "SteamLinuxRuntime_sniper");
        WriteLibraryFolders(steamRoot, libraryPath, sniperAppId);

        // GE Proton and similar builds ship without a toolmanifest.vdf.
        string protonPath = Path.Combine(tempRoot, "GE-Proton10-34");
        Directory.CreateDirectory(protonPath);

        Steam.SteamLibrariesVdf steamLibraries = Steam.SteamLibrariesVdf.Load(steamRoot);
        string? result = steamLibraries.GetRuntimePathForProton(protonPath);

        result.Should().Be(Path.Combine(libraryPath, "steamapps", "common", "SteamLinuxRuntime_sniper"));
    }

    [OSTestMethod(OperatingSystems.Linux)]
    public void GetRuntimePathForProton_ReturnsNull_WhenRequiredRuntimeLibraryIsUnknown()
    {
        string steamRoot = CreateSteamRoot();

        string protonPath = Path.Combine(tempRoot, "proton-cachyos-slr");
        Directory.CreateDirectory(protonPath);
        File.WriteAllText(Path.Combine(protonPath, "toolmanifest.vdf"), """
            "manifest"
            {
                "require_tool_appid" "4183110"
            }
            """);

        Steam.SteamLibrariesVdf steamLibraries = Steam.SteamLibrariesVdf.Load(steamRoot);
        string? result = steamLibraries.GetRuntimePathForProton(protonPath);

        result.Should().BeNull();
    }

    private string CreateSteamRoot()
    {
        string steamRoot = Path.Combine(tempRoot, "steam-root");
        Directory.CreateDirectory(Path.Combine(steamRoot, "config"));
        File.WriteAllText(Path.Combine(steamRoot, "config", "libraryfolders.vdf"), """
            "libraryfolders"
            {
            }
            """);
        return steamRoot;
    }

    private static void WriteLibraryFolders(string steamRoot, string libraryPath, string appId)
    {
        File.WriteAllText(Path.Combine(steamRoot, "config", "libraryfolders.vdf"), $$"""
            "libraryfolders"
            {
                "0"
                {
                    "path"      "{{libraryPath}}"
                    "apps"
                    {
                        "{{appId}}"     "1"
                    }
                }
            }
            """);
    }

    private static void CreateSteamApp(string libraryPath, string appId, string installDir)
    {
        string steamAppsPath = Path.Combine(libraryPath, "steamapps");
        Directory.CreateDirectory(Path.Combine(steamAppsPath, "common", installDir));
        File.WriteAllText(Path.Combine(steamAppsPath, $"appmanifest_{appId}.acf"), $$"""
            "AppState"
            {
                "appid"     "{{appId}}"
                "installdir"        "{{installDir}}"
            }
            """);
    }
}
