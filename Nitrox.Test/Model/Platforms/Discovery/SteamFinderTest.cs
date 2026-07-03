using Nitrox.Model;
using Nitrox.Model.Platforms.Discovery.InstallationFinders;

namespace Nitrox.Test.Model.Platforms.Discovery;

[TestClass]
public class SteamFinderTest
{
    [TestMethod]
    public void SearchAllInstallations_ShouldParseSpaceSeparatedLibraryFoldersVdf()
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

    [TestMethod]
    public void GetLibraryPaths_ShouldIgnoreAppEntriesInLibraryFoldersVdf()
    {
        string tempDir = CreateTempDir();
        try
        {
            string libraryRoot = Path.Combine(tempDir, "SteamLibrary");
            string legacyLibraryRoot = Path.Combine(tempDir, "LegacySteamLibrary");
            string libraryFolders = Path.Combine(tempDir, "libraryfolders.vdf");
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
                                                    "1" "{{legacyLibraryRoot}}"
                                                }
                                                """);

            string[] paths = SteamFinder.GetLibraryPaths(libraryFolders).ToArray();

            paths.Should().Equal(libraryRoot, legacyLibraryRoot);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [TestMethod]
    public void SearchAllInstallations_ShouldHandleRelativeLibraryFoldersPath()
    {
        string tempDir = CreateTempDir();
        string currentDirectory = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = tempDir;
            File.WriteAllText("libraryfolders.vdf", """
                                                    "libraryfolders"
                                                    {
                                                    }
                                                    """);

            Action search = () => SteamFinder.SearchAllInstallations("libraryfolders.vdf", GameInfo.Subnautica.SteamAppId, GameInfo.Subnautica.Name);

            search.Should().NotThrow();
        }
        finally
        {
            Environment.CurrentDirectory = currentDirectory;
            Directory.Delete(tempDir, true);
        }
    }

    private static string CreateTempDir()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"NitroxSteamFinderTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }
}
