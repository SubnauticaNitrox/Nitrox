using System.Text.Json;
using Nitrox.Launcher.Models.Serialization;
using Nitrox.Launcher.Models.Services;
using Nitrox.Model.Serialization;

namespace Nitrox.Launcher.Serialization;

[TestClass]
public class LauncherJsonSerializerContextTests
{
    [TestMethod]
    public void BackupMetadata()
    {
        // Arrange
        BackupMetadata metadata = new()
        {
            NitroxVersion = "1.2.3",
            BackupDate = new DateTime(2026, 11, 14, 12, 0, 0, DateTimeKind.Utc),
            IncludesSaves = true,
            InstallationPath = "/games/nitrox",
            SavesPath = "/games/subnautica/saves"
        };

        const string JSON = """{"NitroxVersion":"1.2.3","BackupDate":"2026-11-14T12:00:00Z","IncludesSaves":true,"InstallationPath":"/games/nitrox","SavesPath":"/games/subnautica/saves"}""";

        // Act
        BackupMetadata? result = JsonSerializer.Deserialize(JSON, LauncherJsonSerializerContext.Default.BackupMetadata);
        string serializedJson = JsonSerializer.Serialize(result, LauncherJsonSerializerContext.Default.BackupMetadata);

        // Assert
        result.Should().BeEquivalentTo(metadata);
        serializedJson.Should().Be(JSON);
    }

    [TestMethod]
    public void SaveFileVersion()
    {
        // Arrange
        SaveFileVersion version = new(new Version(1, 2, 3, 4));

        const string JSON = """{"Major":1,"Minor":2,"Build":3,"Revision":4}""";

        // Act
        SaveFileVersion? result = JsonSerializer.Deserialize(JSON, LauncherJsonSerializerContext.Default.SaveFileVersion);
        string serializedJson = JsonSerializer.Serialize(result, LauncherJsonSerializerContext.Default.SaveFileVersion);

        // Assert
        result.Should().BeEquivalentTo(version);
        serializedJson.Should().Be(JSON);
    }

    [TestMethod]
    public void BlogPostArray()
    {
        // Arrange
        NitroxBlogService.BlogPost[] posts =
        [
            new()
            {
                Released = new DateTimeOffset(2026, 11, 14, 12, 0, 0, TimeSpan.Zero),
                PostUrl = "https://example.test/post",
                Title = new() { Rendered = "Release notes" },
                ThumbnailImageUrl = "https://example.test/image.png"
            }
        ];

        const string JSON = """[{"date":"2026-11-14T12:00:00+00:00","link":"https://example.test/post","title":{"rendered":"Release notes"},"jetpack_featured_media_url":"https://example.test/image.png"}]""";

        // Act
        NitroxBlogService.BlogPost[]? result = JsonSerializer.Deserialize(JSON, LauncherJsonSerializerContext.Default.BlogPostArray);
        string serializedJson = JsonSerializer.Serialize(result, LauncherJsonSerializerContext.Default.BlogPostArray);

        // Assert
        result.Should().BeEquivalentTo(posts);
        serializedJson.Should().Be(JSON);
    }

    [TestMethod]
    public void ChangeLogArray()
    {
        // Arrange
        NitroxWebsiteApiService.ChangeLog[] changeLogs =
        [
            new()
            {
                Released = new DateTimeOffset(2026, 11, 14, 12, 0, 0, TimeSpan.Zero),
                PatchNotes = ["Added generated JSON serialization"],
                Version = "1.2.3"
            }
        ];

        const string JSON = """[{"released":"2026-11-14T12:00:00+00:00","patchnotes":["Added generated JSON serialization"],"version":"1.2.3"}]""";

        // Act
        NitroxWebsiteApiService.ChangeLog[]? result = JsonSerializer.Deserialize(JSON, LauncherJsonSerializerContext.Default.ChangeLogArray);
        string serializedJson = JsonSerializer.Serialize(result, LauncherJsonSerializerContext.Default.ChangeLogArray);

        // Assert
        result.Should().BeEquivalentTo(changeLogs);
        serializedJson.Should().Be(JSON);
    }

    [TestMethod]
    public void NitroxRelease()
    {
        // Arrange
        NitroxWebsiteApiService.NitroxRelease release = new()
        {
            Version = new Version(1, 2, 3),
            Platforms = new Dictionary<string, NitroxWebsiteApiService.PlatformInfo>
            {
                ["test-platform"] = new()
                {
                    FileSize = "1024",
                    Architectures = new Dictionary<string, NitroxWebsiteApiService.ArchitectureInfo>
                    {
                        ["test-architecture"] = new()
                        {
                            DownloadUrl = "https://example.test/nitrox.zip",
                            Md5Hash = "0123456789abcdef",
                            FileSize = "1024"
                        }
                    }
                }
            }
        };

        const string JSON = """{"version":"1.2.3","platforms":{"test-platform":{"filesize":"1024","architectures":{"test-architecture":{"url":"https://example.test/nitrox.zip","md5":"0123456789abcdef","filesize":"1024"}}}}}""";

        // Act
        NitroxWebsiteApiService.NitroxRelease? result = JsonSerializer.Deserialize(JSON, LauncherJsonSerializerContext.Default.NitroxRelease);
        string serializedJson = JsonSerializer.Serialize(result, LauncherJsonSerializerContext.Default.NitroxRelease);

        // Assert
        result.Should().BeEquivalentTo(release, options => options.Excluding(x => x.CurrentPlatformInfo));
        serializedJson.Should().Be(JSON);
    }
}
