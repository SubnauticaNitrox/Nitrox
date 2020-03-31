using System.IO;
using UnityEditor;

namespace Assets.Editor
{
    public class CreateAssetBundles
    {
        [MenuItem("Assets/Build AssetBundles")]
        private static void BuildAllAssetBundles()
        {
            var assetBundleDirectory = "Assets/AssetBundles";
            if (Directory.Exists(assetBundleDirectory))
            {
                Directory.Delete(assetBundleDirectory, true);
            }
            Directory.CreateDirectory(assetBundleDirectory);
            BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None,
                BuildTarget.StandaloneWindows);
        }
    }
}