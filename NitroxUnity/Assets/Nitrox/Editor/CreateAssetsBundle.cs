using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public static class CreateAssetBundles
    {
        private const string UNITY_OUTPUT_DIRECTORY = "AssetBundles";
        private const string NITROX_OUTPUT_DIRECTORY = "../Nitrox.Subnautica.Assets/AssetBundles";

        [MenuItem("Tools/Nitrox/Build AssetBundles")]
        private static void BuildAllAssetBundles()
        {
            EditorUtility.DisplayProgressBar("Building Asset Bundles", "", 0);
            try
            {
                if (Directory.Exists(UNITY_OUTPUT_DIRECTORY))
                {
                    Directory.Delete(UNITY_OUTPUT_DIRECTORY, true);
                }
                Directory.CreateDirectory(UNITY_OUTPUT_DIRECTORY);

                BuildPipeline.BuildAssetBundles(UNITY_OUTPUT_DIRECTORY, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

                // Moves AssetBundles direct to Nitrox.Subnautica.Assets
                if (Directory.Exists(NITROX_OUTPUT_DIRECTORY))
                {
                    foreach (string assetBundle in Directory.GetFiles(UNITY_OUTPUT_DIRECTORY))
                    {
                        string assetBundleName = Path.GetFileName(assetBundle);
                        if (!assetBundleName.EndsWith(".manifest") && !assetBundleName.Equals("AssetBundles"))
                        {
                            File.Copy(assetBundle, Path.Combine(NITROX_OUTPUT_DIRECTORY, assetBundleName), true);
                        }
                    }
                }
                else
                {
                    throw new DirectoryNotFoundException(NITROX_OUTPUT_DIRECTORY + " was not found");
                }

                Debug.Log("Building Nitrox AssetBundles successfully finished");
            }
            catch (Exception ex)
            {
                Debug.LogError("Building Nitrox AssetBundles threw an error.\n" + ex);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
