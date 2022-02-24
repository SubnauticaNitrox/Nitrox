using System;
using System.Collections;
using System.IO;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.Unity.Helper;

public static class AssetBundleLoader
{
    private static readonly string assetRootFolder = NitroxUser.AssetsPath;

    private static bool firstTime = true;

    public static IEnumerator LoadAssetBundle(string bundleName, Action<AssetBundle> callback = null)
    {
        if (firstTime)
        {
            firstTime = false;
            yield return LoadAssetBundle("sharedassets");
        }

        AssetBundleCreateRequest assetRequest = AssetBundle.LoadFromFileAsync(Path.Combine(assetRootFolder, bundleName));
        if (assetRequest == null)
        {
            Log.Error($"Failed to load AssetBundle: {bundleName}");
            yield break;
        }

        yield return assetRequest;

        callback?.Invoke(assetRequest.assetBundle);
    }

    public static IEnumerator LoadUIAsset(string bundleName, bool hideUI, Action<GameObject> callback)
    {
        if (firstTime)
        {
            firstTime = false;
            yield return LoadAssetBundle("sharedassets");
        }

        AssetBundleCreateRequest assetRequest = AssetBundle.LoadFromFileAsync(Path.Combine(assetRootFolder, bundleName));
        if (assetRequest == null)
        {
            Log.Error($"Failed to load AssetBundle: {bundleName}");
            yield break;
        }
        yield return assetRequest;

        AssetBundleRequest fetchAssetRequest = assetRequest.assetBundle.LoadAssetAsync<GameObject>(bundleName);
        yield return fetchAssetRequest;

        GameObject asset = UnityEngine.Object.Instantiate(fetchAssetRequest.asset, uGUI.main.screenCanvas.transform, false) as GameObject;

        if (!asset)
        {
            Log.Error($"Instantiated assetBundle ({bundleName}) but gameObject is null.");
            yield break;
        }

        if (hideUI && asset.TryGetComponent(out CanvasGroup canvasGroup))
        {
            canvasGroup.alpha = 0;
        }

        callback.Invoke(asset);
    }
}
