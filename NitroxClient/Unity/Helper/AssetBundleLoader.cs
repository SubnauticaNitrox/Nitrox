using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NitroxModel.Logger;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.Unity.Helper
{
    public class AssetBundleLoader
    {
        private static readonly string assetRootFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../../AssetBundles");

        public static IEnumerator LoadAsset(string name)
        {
            AssetBundleCreateRequest assetRequest = AssetBundle.LoadFromFileAsync(Path.Combine(assetRootFolder, name));
            if (assetRequest == null)
            {
                Log.Error("Failed to load AssetBundle!");
                yield break;
            }

            while (!assetRequest.isDone)
            {
                yield return null;
            }

            string sceneName = assetRequest.assetBundle.GetAllScenePaths().First();
            Log.Debug($"Trying to load scene: {sceneName}");
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        }

        public static IEnumerator LoadUIAsset(string name, string canvasObjectName)
        {
            yield return LoadAsset(name);

            GameObject assetCanvas = GameObject.Find(canvasObjectName);
            Transform assetRoot = assetCanvas.transform.GetChild(0);
            assetRoot.SetParent(uGUI.main.screenCanvas.transform, false);
            Object.Destroy(assetCanvas);

            yield return assetRoot.gameObject;
        }
    }
}
