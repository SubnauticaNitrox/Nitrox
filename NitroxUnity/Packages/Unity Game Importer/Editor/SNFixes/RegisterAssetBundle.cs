using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PassivePicasso.GameImporter.SN_Fixes;
using UnityEditor;
using UnityEngine;

namespace Packages.ThunderKit.GameImporter.Editor.SNFixes
{
    public class RegisterAssetBundle : ISNFix
    {
        public string GetTaskName() => TASK_NAME;
        public const string TASK_NAME = "Register all Assets in Asset Bundles";

        public void Run() => RegisterAssetsAsBundle();

        private static readonly HashSet<Type> whitelistedAssetTypes = new HashSet<Type>()
        {
            typeof(Material),
            typeof(Texture2D),
            typeof(Shader),
            typeof(Font)
        };

        public static void RegisterAssetsAsBundle()
        {
            string[] files = Directory.GetFiles(SNFixesUtility.AssetPath, "*.meta", SearchOption.AllDirectories)
                                      .Where(file => !file.EndsWith(".unity.meta")).ToArray();
            for (int index = 0; index < files.Length; index++)
            {
                string filePath = "Assets" + files[index].Substring(Application.dataPath.Length, files[index].Length - Application.dataPath.Length - 5);
                Type assetType = AssetDatabase.GetMainAssetTypeAtPath(filePath);

                if (Directory.Exists(filePath))
                {
                    continue;
                }

                if (!whitelistedAssetTypes.Contains(assetType))
                {
                    AssetImporter.GetAtPath(filePath).SetAssetBundleNameAndVariant("", "");
                    continue;
                }

                SNFixesUtility.ProgressBar.Update(Path.GetFileName(filePath), "Adding Assets to AssetBundle", (float)index / files.Length);
                AssetImporter.GetAtPath(filePath).SetAssetBundleNameAndVariant($"SubnauticaAssets-{assetType.Name}", "");
            }
        }
    }
}
