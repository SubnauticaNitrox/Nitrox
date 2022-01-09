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

    private static IEnumerator LoadAssetBundle(string bundleName, Action<AssetBundle> callback = null)
    {
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
            Log.Debug("1");
            yield return LoadAssetBundle("sharedassets");
            Log.Debug("2");
        }

        Log.Debug("3");
        AssetBundleCreateRequest assetRequest = AssetBundle.LoadFromFileAsync(Path.Combine(assetRootFolder, bundleName));
        if (assetRequest == null)
        {
            Log.Error($"Failed to load AssetBundle: {bundleName}");
            yield break;
        }

        Log.Debug("4");
        yield return assetRequest;

        Log.Debug("5");
        AssetBundleRequest fetchAssetRequest = assetRequest.assetBundle.LoadAssetAsync<GameObject>(bundleName);
        yield return fetchAssetRequest;

        Log.Debug("6");
        GameObject asset = UnityEngine.Object.Instantiate(fetchAssetRequest.asset, uGUI.main.screenCanvas.transform, false) as GameObject;

        Log.Debug("7");
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
