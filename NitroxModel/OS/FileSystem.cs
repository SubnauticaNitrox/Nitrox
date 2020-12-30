﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public virtual IEnumerable<string> GetDefaultPrograms(string file)
        {
            throw new NotSupportedException();
        }

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
            string arguments = Path.GetFileName(editorProgram).ToLowerInvariant() switch
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
    }
}
