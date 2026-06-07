using Nitrox.Model;
using Nitrox.Model.Platforms.Discovery;
using Nitrox.Model.Platforms.Discovery.InstallationFinders;
using Nitrox.Test.Model.Platforms;

namespace Nitrox.Test.Model.Platforms.Discovery;

[TestClass]
public class GameInstallationHelperTest
{
    [OSTestMethod(OperatingSystems.OSX)]
    public void NormalizeGamePath_ShouldAcceptMacOSAppBundle()
    {
        string tempDir = CreateTempDir();
        try
        {
            string steamGameRoot = Path.Combine(tempDir, "steamapps", "common", "Subnautica");
            string appContents = Path.Combine(steamGameRoot, "Subnautica.app", "Contents");
            CreateNativeMacSubnautica(appContents);

            string normalizedFromRoot = GameInstallationHelper.NormalizeGamePath(steamGameRoot, GameInfo.Subnautica);
            string normalizedFromBundle = GameInstallationHelper.NormalizeGamePath(Path.Combine(steamGameRoot, "Subnautica.app"), GameInfo.Subnautica);

            normalizedFromRoot.Should().Be(appContents);
            normalizedFromBundle.Should().Be(appContents);
            GameInstallationHelper.HasValidGameFolder(normalizedFromBundle, GameInfo.Subnautica).Should().BeTrue();
            GameInstallationHelper.IsNativeMacOSGameLayout(normalizedFromBundle, GameInfo.Subnautica).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [OSTestMethod(OperatingSystems.OSX)]
    public void NormalizeGamePath_ShouldAcceptWindowsWineLayoutOnMacOS()
    {
        string tempDir = CreateTempDir();
        try
        {
            string gameRoot = Path.Combine(tempDir, "drive_c", "Program Files (x86)", "Steam", "steamapps", "common", "Subnautica");
            CreateWindowsSubnautica(gameRoot);

            string normalized = GameInstallationHelper.NormalizeGamePath(gameRoot, GameInfo.Subnautica);

            normalized.Should().Be(gameRoot);
            GameInstallationHelper.HasValidGameFolder(normalized, GameInfo.Subnautica).Should().BeTrue();
            GameInstallationHelper.IsWindowsGameLayout(normalized, GameInfo.Subnautica).Should().BeTrue();
            GameInstallationHelper.TryGetGameExecutablePath(normalized, GameInfo.Subnautica, out string executablePath).Should().BeTrue();
            executablePath.Should().EndWith(Path.Combine("Subnautica", "Subnautica.exe"));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [TestMethod]
    public void SteamFinder_ShouldParseModernLibraryFoldersVdf()
    {
        string tempDir = CreateTempDir();
        try
        {
            string libraryRoot = Path.Combine(tempDir, "SteamLibrary");
            Directory.CreateDirectory(Path.Combine(libraryRoot, "steamapps"));
            File.WriteAllText(Path.Combine(libraryRoot, "steamapps", $"appmanifest_{GameInfo.Subnautica.SteamAppId}.acf"), "");

            string steamApps = Path.Combine(tempDir, "Steam", "steamapps");
            Directory.CreateDirectory(steamApps);
            string libraryFolders = Path.Combine(steamApps, "libraryfolders.vdf");
            File.WriteAllText(libraryFolders, $$"""
                                                "libraryfolders"
                                                {
                                                    "0"
                                                    {
                                                        "path" "{{libraryRoot}}"
                                                        "apps"
                                                        {
                                                            "{{GameInfo.Subnautica.SteamAppId}}" "1"
                                                        }
                                                    }
                                                }
                                                """);

            string? path = SteamFinder.SearchAllInstallations(libraryFolders, GameInfo.Subnautica.SteamAppId, GameInfo.Subnautica.Name);

            path.Should().Be(Path.Combine(libraryRoot, "steamapps", "common", GameInfo.Subnautica.Name));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [OSTestMethod(OperatingSystems.OSX)]
    public void WineFinder_ShouldIncludeCommonMacOSPrefixes()
    {
        string tempDir = CreateTempDir();
        try
        {
            string crossoverBottle = Path.Combine(tempDir, "Library", "Application Support", "CrossOver", "Bottles", "Steam");
            string winePrefix = Path.Combine(tempDir, ".wine");
            Directory.CreateDirectory(Path.Combine(crossoverBottle, "drive_c"));
            Directory.CreateDirectory(Path.Combine(winePrefix, "drive_c"));

            List<string> prefixes = WineFinder.GetCandidatePrefixes(tempDir).ToList();
            prefixes.Should().Contain(crossoverBottle);
            prefixes.Should().Contain(winePrefix);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static string CreateTempDir()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"NitroxTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }

    private static void CreateNativeMacSubnautica(string contentsPath)
    {
        Directory.CreateDirectory(Path.Combine(contentsPath, "MacOS"));
        Directory.CreateDirectory(Path.Combine(contentsPath, "Resources", "Data", "Managed"));
        File.WriteAllText(Path.Combine(contentsPath, "MacOS", "Subnautica"), "");
    }

    private static void CreateWindowsSubnautica(string gameRoot)
    {
        Directory.CreateDirectory(Path.Combine(gameRoot, "Subnautica_Data", "Managed"));
        File.WriteAllText(Path.Combine(gameRoot, "Subnautica.exe"), "");
    }
}
