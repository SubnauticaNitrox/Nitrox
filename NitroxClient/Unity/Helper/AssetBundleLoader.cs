using System;
using System.Collections;
using System.IO;
using NitroxModel.Helper;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.Unity.Helper
{
    public class AssetBundleLoader
    {
        private static readonly string assetRootFolder = NitroxAppData.Instance.AssetsPath;

        public static IEnumerator LoadAsset(string name, Action<GameObject> callback)
        {
            string assetBundlePath = Path.Combine(assetRootFolder, name);
            AssetBundleCreateRequest assetRequest = AssetBundle.LoadFromFileAsync(assetBundlePath);
            if (assetRequest == null)
            {
                Log.Error($"Failed to load AssetBundle form {assetBundlePath}");
                yield break;
            }
            yield return assetRequest;

            AssetBundleRequest assetLoadRequest = assetRequest.assetBundle.LoadAssetAsync<GameObject>($"Assets/Nitrox/AssetBundles/{name}/{name}.prefab");
            if (assetLoadRequest == null)
            {
                Log.Error($"Failed to load {name}.prefab from AssetBundle at {assetBundlePath}");
                yield break;
            }
            yield return assetLoadRequest;

            callback?.Invoke((GameObject)GameObject.Instantiate(assetLoadRequest.asset, null));
        }

        public static IEnumerator LoadUIAsset(string name, bool hideUI = false, Action<GameObject> callback = null)
        {
            yield return LoadAsset(name, asset =>
            {
                if (hideUI)
                {
                    asset.GetComponent<CanvasGroup>().alpha = 0;
                    asset.SetActive(true);
                }
                asset.transform.SetParent(uGUI.main.screenCanvas.transform, false);

                callback?.Invoke(asset);
            });
        }
    }
}
