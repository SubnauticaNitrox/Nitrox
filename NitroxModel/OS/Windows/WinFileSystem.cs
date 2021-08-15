using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace NitroxModel.OS.Windows
{
    internal class WinFileSystem : FileSystem
    {
        public override IEnumerable<string> ExecutableFileExtensions { get; } = new[] { "exe", "cmd", "bat" };
        public override string TextEditor => GetFullPath("notepad.exe");

        public override IEnumerable<string> GetDefaultPrograms(string file)
        {
            string SearchExecutableInSameDirectory(string path)
            {
                if (Path.GetExtension(path) != "")
                {
                    return path;
                }

                foreach (string ext in ExecutableFileExtensions)
                {
                    string newPath = Path.ChangeExtension(path, ext);
                    if (File.Exists(newPath))
                    {
                        return newPath;
                    }
                }

                return path;
            }

            string extension = Path.GetExtension(file);
            if (string.IsNullOrWhiteSpace(extension))
            {
                yield break;
            }

            string defaultProgramOnDblClick = Native.AssocQueryString(Native.AssocStr.Executable, extension);
            if (!string.IsNullOrWhiteSpace(defaultProgramOnDblClick) && File.Exists(defaultProgramOnDblClick))
            {
                yield return Path.GetFullPath(defaultProgramOnDblClick);
            }

            string baseKey = $@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{extension}";
            using RegistryKey rk = Registry.CurrentUser.OpenSubKey($@"{baseKey}\OpenWithList");
            if (rk?.GetValue("MRUList") is not string mruList)
            {
                yield break;
            }

            foreach (char c in mruList)
            {
                string fullPath = SearchExecutableInSameDirectory(GetFullPath(rk.GetValue(c.ToString()).ToString()));
                if (fullPath == null)
                {
                    continue;
                }

                yield return fullPath;
            }
        }
    }
}
