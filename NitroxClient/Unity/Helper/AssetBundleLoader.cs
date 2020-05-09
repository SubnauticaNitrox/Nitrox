using System;
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
            yield return new WaitUntil(() => assetRequest.isDone);

            string sceneName = assetRequest.assetBundle.GetAllScenePaths().First();
            Log.Debug($"Trying to load scene: {sceneName}");
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        }

        public static IEnumerator LoadUIAsset(string name, string canvasObjectName, bool hideUI = false, Action<GameObject> callback = null)
        {
            yield return LoadAsset(name);

            GameObject assetCanvas = GameObject.Find(canvasObjectName);
            Transform assetRoot = assetCanvas.transform.GetChild(0);
            if (hideUI)
            {
                assetRoot.GetComponent<CanvasGroup>().alpha = 0;
            }
            assetRoot.SetParent(uGUI.main.screenCanvas.transform, false);
            UnityEngine.Object.Destroy(assetCanvas);

            if (callback != null)
            {
                callback.Invoke(assetRoot.gameObject);
            }
        }
    }
}
