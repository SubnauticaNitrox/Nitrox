using System.IO;
using UnityEditor;

namespace Assets.Editor
{
    public class CreateAssetBundles
    {
        private const string UNITY_DIRECTORY = "AssetBundles";
        private const string NITROX_DIRECTORY = "../AssetBundles";

        [MenuItem("Nitrox/Build AssetBundles")]
        private static void BuildAllAssetBundles()
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
                    if (File.Exists($"{UNITY_DIRECTORY}/{assetBundleName}"))
                    {
                        File.Copy($"{UNITY_DIRECTORY}/{assetBundleName}", $"{NITROX_DIRECTORY}/{assetBundleName}", true);
                    }
                }

            }
        }
    }
}
