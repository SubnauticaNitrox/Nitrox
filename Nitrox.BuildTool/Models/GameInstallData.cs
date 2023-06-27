using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxModel.Discovery;

/// <summary>
///     Game definition data gathered from Steam's files.
/// </summary>
[Serializable]
public class GameInstallData
{
    public const string XML_GAME_DIR = "GameDir";
    public const string XML_GAME_MANAGED_DIR = "GameManagedDir";

    private string managedDllsDir;
    public string InstallDir { get; private set; }

    public string ManagedDllsDir => managedDllsDir ??= Directory.EnumerateDirectories(InstallDir, "Managed", SearchOption.AllDirectories).FirstOrDefault();

    protected GameInstallData()
    {
        // Required for serialization
    }

    public GameInstallData(string installDir)
    {
        Validate.NotNull(installDir, $"Argument '{nameof(installDir)}' must not be null.");
        InstallDir = installDir;
    }

    public static bool TryRead(string path, out GameInstallData result)
    {
        try
        {
            GameInstallData game = new();
            XmlDocument xDoc = new();
            xDoc.Load(path);
            XmlNamespaceManager nsManager = new(xDoc.NameTable);
            nsManager.AddNamespace("d", xDoc.DocumentElement.NamespaceURI);
            foreach (XmlElement elem in xDoc.DocumentElement.SelectNodes("//d:PropertyGroup/*", nsManager))
            {
                switch (elem.Name)
                {
                    case XML_GAME_DIR:
                        game.InstallDir = elem.LastChild.Value;
                        break;

                    case XML_GAME_MANAGED_DIR:
                        if (!Directory.Exists(elem.LastChild.Value))
                        {
                            throw new DirectoryNotFoundException();
                        }
                        game.managedDllsDir = elem.LastChild.Value;
                        break;
                }
            }

            result = game;
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occured while reading game install data XML");
            result = null;
            return false;
        }
    }

    public void TrySave(string path)
    {
        static string PostfixBackslash(string text)
        {
            return text switch
            {
                null => null,
                "" => "\\",
                _ => text.TrimEnd('\\') + '\\'
            };
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using XmlTextWriter writer = new(path, Encoding.UTF8)
        {
            Formatting = Formatting.Indented
        };
        writer.WriteStartElement("Project");
        writer.WriteAttributeString("xmlns", "http://schemas.microsoft.com/developer/msbuild/2003");
        writer.WriteStartElement("PropertyGroup");
        writer.WriteElementString(XML_GAME_DIR, PostfixBackslash(InstallDir));
        writer.WriteElementString(XML_GAME_MANAGED_DIR, PostfixBackslash(ManagedDllsDir));
        writer.WriteEndElement();
        writer.WriteEndElement();
    }
}
