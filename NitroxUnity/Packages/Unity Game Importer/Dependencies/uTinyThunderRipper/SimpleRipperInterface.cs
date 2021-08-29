#if DEBUG
#define DEBUG_PROGRAM
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using uTinyRipper;
using uTinyRipper.Classes;
using uTinyRipper.Converters;
using Version = uTinyRipper.Version;
using Logger = uTinyRipper.Logger;
using LogType = uTinyRipper.LogType;
using UnityEngine;
using uTinyRipperGUI.Exporters;
using ILogger = uTinyRipper.ILogger;

namespace ThunderKit.uTinyRipper
{
    public class SimpleRipperInterface : ScriptableObject
    {
        internal static void Main(string[] args)
        {
            Logger.Instance = ConsoleLogger.Instance;

            if (args.Length == 0)
            {
                Console.WriteLine("No arguments");
                Console.ReadKey();
                return;
            }

            foreach (string arg in args)
            {
                if (arg.StartsWith("--Types=")) continue;
                if (MultiFileStream.Exists(arg))
                {
                    continue;
                }
                if (DirectoryUtils.Exists(arg))
                {
                    continue;
                }
                Console.WriteLine(MultiFileStream.IsMultiFile(arg) ?
                    $"File '{arg}' doesn't have all parts for combining" :
                    $"Neither file nor directory with path '{arg}' exists");
                Console.ReadKey();
                return;
            }

            var classes = args
                .FirstOrDefault(arg => arg.StartsWith("--Types="))
                .Substring("--Types=".Length)
                .Split(',')
                .Select(cls =>
                {
                    if (Enum.TryParse<ClassIDType>(cls, out var result))
                        return result;
                    return (ClassIDType)(-1);
                })
                .Where(v => ((int)(v)) >= 0)
                .ToList();

            classes.Add(ClassIDType.GraphicsSettings);
            classes.Add(ClassIDType.NavMeshSettings);
            classes.Add(ClassIDType.PlayerSettings);
            classes.Add(ClassIDType.QualitySettings);
            classes.Add(ClassIDType.RenderSettings);
            classes.Add(ClassIDType.SceneSettings);
            classes.Add(ClassIDType.EditorSettings);
            classes.Add(ClassIDType.EditorBuildSettings);
            classes.Add(ClassIDType.EditorUserBuildSettings);
            classes.Add(ClassIDType.LightmapSettings);
            classes.Add(ClassIDType.Physics2DSettings);
            classes.Clear();

            PrepareExportDirectory(Path.Combine("Ripped", "Assets"));
            PrepareExportDirectory(Path.Combine("Ripped", "ProjectSettings"));

            SimpleRipperInterface program = new SimpleRipperInterface();
            program.Load(args[0], classes, Platform.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags);
            Console.ReadKey();
        }

        NoExporter noExporter = new NoExporter();

        public void Load(string gameDir, IEnumerable<ClassIDType> classes, Platform platform, TransferInstructionFlags transferInstructionFlags, ILogger logger = null)
        {
            try
            {
                Logger.Instance = logger;
                var filename = Path.GetFileName(gameDir);
                var player = Path.Combine(gameDir, $"{filename}.exe");
                var playerInfo = FileVersionInfo.GetVersionInfo(player);
                var unityVersion = playerInfo.ProductVersion;
                unityVersion = unityVersion.Substring(0, unityVersion.LastIndexOf('.'));

                var gameStructure = GameStructure.Load(new[] { gameDir });
                var fileCollection = gameStructure.FileCollection;
                ShutItAllDown(fileCollection.Exporter);
                fileCollection.Exporter.OverrideExporter(ClassIDType.MonoScript, new NoScriptExporter());

                Logger.Log(LogType.Info, LogCategory.General, "Loading Class Types export configuration");
                foreach (var cls in classes)
                    EnableExport(cls, fileCollection.Exporter);

                Dictionary<string, Guid> AssemblyHash = new Dictionary<string, Guid>();
                Dictionary<string, long> ScriptId = new Dictionary<string, long>();

                var unityPlayerVersion = new System.Version(unityVersion);
                var version = new Version(unityPlayerVersion.Major, unityPlayerVersion.Minor, unityPlayerVersion.Revision, VersionType.Final, 3);

                var options = new ExportOptions(
                    version,
                    platform,
                    transferInstructionFlags
                );
                var serializedFiles = fileCollection.GameFiles.Values;

                string path = Directory.GetCurrentDirectory();
                fileCollection.Exporter.Export(path, fileCollection, serializedFiles, options);

                Logger.Log(LogType.Info, LogCategory.General, "Finished");
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, LogCategory.General, ex.ToString());
            }
        }

        private void EnableExport(ClassIDType cls, ProjectExporter exporter)
        {
            TextureAssetExporter textureExporter = new TextureAssetExporter();

            switch (cls)
            {
                case ClassIDType.Shader:
                    exporter.OverrideBinaryExporter(cls);
                    break;

                case ClassIDType.AudioClip:
                    exporter.OverrideExporter(cls, new AudioAssetExporter());
                    break;

                case ClassIDType.MovieTexture:
                    exporter.OverrideExporter(cls, new MovieTextureAssetExporter());
                    break;

                case ClassIDType.TextAsset:
                    exporter.OverrideExporter(cls, new TextAssetExporter());
                    break;

                case ClassIDType.Font:
                    exporter.OverrideExporter(cls, new FontAssetExporter());
                    break;

                case ClassIDType.MonoScript:
                    break;

                case ClassIDType.MonoManager:
                case ClassIDType.AssetBundle:
                case ClassIDType.ResourceManager:
                case ClassIDType.PreloadData:
                    exporter.OverrideDummyExporter(cls, true, false);
                    break;

                case ClassIDType.Texture2D:
                case ClassIDType.Cubemap:
                case ClassIDType.Sprite:
                    exporter.OverrideExporter(cls, textureExporter);
                    break;

                case ClassIDType.BuildSettings:
                case ClassIDType.EditorSettings:
                case ClassIDType.TextureImporter:
                case ClassIDType.DefaultAsset:
                case ClassIDType.DefaultImporter:
                case ClassIDType.NativeFormatImporter:
                case ClassIDType.MonoImporter:
                case ClassIDType.DDSImporter:
                case ClassIDType.PVRImporter:
                case ClassIDType.ASTCImporter:
                case ClassIDType.KTXImporter:
                case ClassIDType.IHVImageFormatImporter:
                case ClassIDType.SpriteAtlas:
                    exporter.OverrideDummyExporter(cls, false, false);
                    break;

                default:
                    exporter.OverrideYamlExporter(cls);
                    break;
            }
        }

        private static void PrepareExportDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                //DirectoryUtils.Delete(path, true);
            }
        }

        private void ShutItAllDown(ProjectExporter exporter)
        {
            foreach (ClassIDType cls in Enum.GetValues(typeof(ClassIDType)).OfType<ClassIDType>())
                if (cls == ClassIDType.MonoScript) continue;
                else
                    exporter.OverrideExporter(cls, noExporter);
        }

    }
}
