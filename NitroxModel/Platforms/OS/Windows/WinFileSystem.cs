using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.OS.Windows.Internal;

namespace NitroxModel.Platforms.OS.Windows
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

            string defaultProgramOnDblClick = Win32Native.AssocQueryString(Win32Native.AssocStr.Executable, extension);
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

        /// <summary>
        ///     Adds full access flag to the directory (and sub files/directories) for the current user.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns>True if set, false if program is not allowed to change permissions.</returns>
        public override bool SetFullAccessToCurrentUser(string directory)
        {
            try
            {
                string identity = WindowsIdentity.GetCurrent().Name;

                DirectoryInfo dir = new(directory);
                DirectorySecurity flags = dir.GetAccessControl();
                flags.AddAccessRule(new(identity, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.InheritOnly, AccessControlType.Allow));
                flags.AddAccessRule(new(identity, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
                flags.AddAccessRule(new(identity, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
                dir.SetAccessControl(flags);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public override bool IsTrustedFile(string file) => Win32Native.IsTrusted(file);

        private static bool DeleteFileOrFolder(string path)
        {
            try
            {
                // https://learn.microsoft.com/en-us/windows/win32/api/shellapi/ns-shellapi-shfileopstructa
                Win32Native.SHFILEOPSTRUCT fileop = new()
                {
                    wFunc = Win32Native.FO_DELETE,
                    pFrom = $"{path}\0",
                    fFlags = Win32Native.FOF_ALLOWUNDO | Win32Native.FOF_NOCONFIRMATION
                };

                var rc = Win32Native.SHFileOperation(ref fileop);
                return rc == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override bool MoveDirectoryToRecycleBin(DirectoryInfo directoryInfo)
        {
            if (directoryInfo is null) return false;

            directoryInfo.Refresh();
            if (!directoryInfo.Exists)
            {
                return false;
            }

            return DeleteFileOrFolder(directoryInfo.FullName);
        }

        public override bool MoveFileToRecycleBin(FileInfo fileInfo)
        {
            if (fileInfo is null) return false;

            fileInfo.Refresh();
            if (!fileInfo.Exists)
            {
                return false;
            }

            return DeleteFileOrFolder(fileInfo.FullName);
        }
    }
}
