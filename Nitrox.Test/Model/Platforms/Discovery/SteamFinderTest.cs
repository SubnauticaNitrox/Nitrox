using Nitrox.Model;
using Nitrox.Model.Platforms.Discovery.InstallationFinders;

namespace Nitrox.Test.Model.Platforms.Discovery;

[TestClass]
public class SteamFinderTest
{
    [TestMethod]
    public void SearchAllInstallations_ShouldParseModernLibraryFoldersVdf()
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

    private static string CreateTempDir()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"NitroxSteamFinderTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }
}
