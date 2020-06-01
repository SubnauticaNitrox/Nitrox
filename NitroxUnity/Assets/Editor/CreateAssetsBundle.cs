using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public class CreateAssetBundles
    {
        private const string UNITY_DIRECTORY = "AssetBundles";
        private const string NITROX_DIRECTORY = "../Nitrox.Subnautica.Assets/AssetBundles";

        [MenuItem("Nitrox/Build AssetBundles")]
        private static void BuildAllAssetBundles()
        {
            try
            {
                if (Directory.Exists(UNITY_DIRECTORY))
                {
                    Directory.Delete(UNITY_DIRECTORY, true);
                }
                Directory.CreateDirectory(UNITY_DIRECTORY);

                BuildPipeline.BuildAssetBundles(UNITY_DIRECTORY, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

                if (Directory.Exists(NITROX_DIRECTORY))
                {
                    foreach (string assetBundleName in AssetDatabase.GetAllAssetBundleNames())
                    {
                        File.Copy(Path.Combine(UNITY_DIRECTORY, assetBundleName), Path.Combine(NITROX_DIRECTORY, assetBundleName), true);
                    }
                }
                else
                {
                    throw new DirectoryNotFoundException(NITROX_DIRECTORY + " wasn't found");
                }
                Debug.Log("Building Nitrox AssetBundles successfully finished");
            }
            catch (Exception ex)
            {
                Debug.LogError("Building Nitrox AssetBundles successfully finished");
                Debug.LogException(ex);
            }

        }
    }
}
