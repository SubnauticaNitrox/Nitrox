using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using NitroxModel.OS.MacOS;
using NitroxModel.OS.Unix;
using NitroxModel.OS.Windows;

namespace NitroxModel.OS
{
    public class FileSystem
    {
        private static readonly Lazy<FileSystem> instance = new(() =>
                                                                {
                                                                    return Environment.OSVersion.Platform switch
                                                                    {
                                                                        PlatformID.Unix => new UnixFileSystem(),
                                                                        PlatformID.MacOSX => new MacFileSystem(),
                                                                        _ => new WinFileSystem()
                                                                    };
                                                                },
                                                                LazyThreadSafetyMode.ExecutionAndPublication);

        public virtual IEnumerable<string> ExecutableFileExtensions => throw new NotSupportedException();
        public static FileSystem Instance => instance.Value;
        public virtual string TextEditor => throw new NotSupportedException();

        protected FileSystem()
        {
        }

        public virtual IEnumerable<string> GetDefaultPrograms(string file) => throw new NotSupportedException();

        /// <summary>
        ///     Opens the file with the default associated program or the default editor of the OS.
        ///     The returned <see cref="Process" /> should be disposed.
        /// </summary>
        /// <param name="file">File or program to open or execute.</param>
        /// <returns>Instance of a running process. Should be disposed.</returns>
        public virtual Process OpenOrExecuteFile(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new ArgumentException("File path must not be null or empty.", nameof(file));
            }

            string editorProgram = GetDefaultPrograms(file).FirstOrDefault() ?? TextEditor;

            // Handle special arguments for popular editors.
            string arguments = Path.GetFileName(editorProgram)?.ToLowerInvariant() switch
            {
                "code.cmd" => "--wait", // Allow to wait on VS code
                _ => ""
            };

            Process process = new();
            process.StartInfo.UseShellExecute = false;
            // Suppress text output by redirecting it away from main console window.
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.FileName = editorProgram;
            process.StartInfo.Arguments = $@"{(arguments.Length > 0 ? arguments + " " : "")}""{file}""";
            process.Start();
            return process;
        }

        /// <summary>
        ///     Gets the full path to a file or program. Searches the PATH environment variables if file could not be found
        ///     relatively. Returns null if not found.
        /// </summary>
        /// <param name="fileName">File or program name to get the full path from.</param>
        /// <returns></returns>
        public string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
            {
                return Path.GetFullPath(fileName);
            }
            string values = Environment.GetEnvironmentVariable("PATH");
            if (values == null)
            {
                return null;
            }

            fileName = Path.GetFileName(fileName);
            // Always test filename in system lib root first, then other paths. On UNIX systems the path is case-sensitive.
            IEnumerable<string> pathsToTools = new[] { Environment.SystemDirectory }.Concat(values.Split(Path.PathSeparator)).Distinct();
            foreach (string path in pathsToTools)
            {
                string fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
            return null;
        }

        public string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException(nameof(fromPath));
            }
            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException(nameof(toPath));
            }
            // Ensure postfix so that result becomes relative to entire "from" path.
            fromPath = fromPath[fromPath.Length - 1] == Path.DirectorySeparatorChar ? fromPath : fromPath + Path.DirectorySeparatorChar;
            Uri fromUri = new(fromPath);
            Uri toUri = new(toPath);
            // Can path be made relative?
            if (fromUri.Scheme != toUri.Scheme)
            {
                return toPath;
            }

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
            return relativePath;
        }

        /// <summary>
        ///     Zips the files found in the given directory.
        /// </summary>
        /// <param name="dir">Directory to search for files to zip.</param>
        /// <param name="outputPath">
        ///     Name of output zip, optionally including full path. If null, uses the directory name given by
        ///     <see cref="dir" />.
        /// </param>
        /// <param name="fileSearchPattern">Search pattern used to filter against files to zip.</param>
        /// <param name="replaceFile">If true, overwrites the file matching the output path.</param>
        /// <returns>Full path to the newly created zip or null if no files are found to zip.</returns>
        /// <exception cref="IOException">If zip file already exists.</exception>
        public string ZipFilesInDirectory(string dir, string outputPath = null, string fileSearchPattern = "*", bool replaceFile = false)
        {
            if (string.IsNullOrWhiteSpace(dir))
            {
                throw new ArgumentException("Directory must not be null or empty", nameof(dir));
            }
            dir = Path.GetFullPath(dir);
            if (!Directory.Exists(dir))
            {
                throw new ArgumentException("Path is not a directory", nameof(dir));
            }
            // Figure out relative path of output OR use <basename>.zip of directory.
            outputPath = Path.GetFullPath(outputPath ?? dir);
            string outZipName = Path.GetFileName(outputPath);
            if (string.IsNullOrEmpty(Path.GetExtension(outZipName)))
            {
                outZipName = Path.ChangeExtension(outZipName, ".zip");
            }
            string outZipDir = Path.GetDirectoryName(outputPath) ?? dir;
            string outZipFullName = Path.Combine(outZipDir, outZipName);
            if (!replaceFile && File.Exists(outZipFullName))
            {
                throw new IOException($"The file '{outZipFullName}' already exists");
            }
            string[] files = Directory.GetFiles(dir, fileSearchPattern, SearchOption.AllDirectories);
            if (files.Length < 1)
            {
                return null;
            }

            // Create the zip.
            Directory.CreateDirectory(outZipDir);
            using ZipArchive zip = new(File.Create(outZipFullName), ZipArchiveMode.Create);
            foreach (string file in files)
            {
                ZipArchiveEntry entry = zip.CreateEntry(MakeRelativePath(dir, file));
                using Stream sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                using Stream targetStream = entry.Open();
                sourceStream.CopyTo(targetStream);
            }

            return outZipFullName;
        }

        /// <summary>
        ///     Replaces target file with source file. If target file does not exist then it moves the file.
        /// </summary>
        /// <param name="source">Source file to replace with.</param>
        /// <param name="target">Target file to replace.</param>
        public void ReplaceFile(string source, string target)
        {
            if (!File.Exists(target))
            {
                File.Move(source, target);
            }
            else
            {
                File.Replace(source, target, null, false);
            }
        }
    }
}
