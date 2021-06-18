using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NitroxModel.Helper;

namespace NitroxModel.Discovery
{
    /// <summary>
    ///     Game definition data gathered from Steam's files.
    /// </summary>
    public class GameInstallData
    {
        private string managedDllsDir;
        public string InstallDir { get; private set; }

        public string ManagedDllsDir => managedDllsDir ??= Directory.EnumerateDirectories(InstallDir, "Managed", SearchOption.AllDirectories).FirstOrDefault();

        private GameInstallData()
        {
            // Required for serialization
        }

        public GameInstallData(string installDir)
        {
            Validate.NotNull(installDir, $"Argument '{nameof(installDir)}' must not be null.");
            InstallDir = installDir;
        }

        public static bool TryFrom(string path, out GameInstallData result)
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
                        case "GameDir":
                            game.InstallDir = elem.LastChild.Value;
                            break;
                        case "GameManagedDir":
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
            catch (Exception)
            {
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
            writer.WriteElementString("GameDir", PostfixBackslash(InstallDir));
            writer.WriteElementString("GameManagedDir", PostfixBackslash(ManagedDllsDir));
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }
}
