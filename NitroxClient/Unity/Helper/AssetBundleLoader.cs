using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.Unity.Helper;

public static class AssetBundleLoader
{
    private static readonly string assetRootFolder = NitroxUser.AssetsPath;

    // Two operations should not run at the same time
    public static bool Working = false;
    private readonly static HashSet<string> loadedBundles = new();

    public static IEnumerator LoadAssetBundle(NitroxAssetBundle nitroxAssetBundle, Action<AssetBundle> callback = null)
    {
        yield return LoadAssetBundle(nitroxAssetBundle.BundleName, callback);
    }

    public static IEnumerator LoadAssetBundle(string bundleName, Action<AssetBundle> callback = null)
    {
        if (HasBundleLoaded(bundleName))
        {
            yield break;
        }
        if (Working)
        {
            yield return new WaitUntil(() => !Working);
        }
        // This must also happen after the Working check because it's only when this thread will work that the previous one could be added to the loadedBundles list
        if (HasBundleLoaded(bundleName))
        {
            yield break;
        }

        Working = true;

        AssetBundleCreateRequest assetRequest = AssetBundle.LoadFromFileAsync(Path.Combine(assetRootFolder, bundleName));
        if (assetRequest == null)
        {
            Log.Error($"Failed to load AssetBundle: {bundleName}");
            Working = false;
            yield break;
        }
        
        yield return assetRequest;

        callback?.Invoke(assetRequest.assetBundle);
        loadedBundles.Add(bundleName);
        Working = false;
    }

    public static IEnumerator LoadUIAsset(NitroxAssetBundle nitroxAssetBundle, bool hideUI, Action<GameObject> callback)
    {
        yield return LoadUIAsset(nitroxAssetBundle.BundleName, hideUI, callback);
    }

    public static IEnumerator LoadUIAsset(string bundleName, bool hideUI, Action<GameObject> callback)
    {
        if (HasBundleLoaded(bundleName))
        {
            yield break;
        }
        if (Working)
        {
            yield return new WaitUntil(() => !Working);
        }
        // Same reason as in LoadAssetBundle
        if (HasBundleLoaded(bundleName))
        {
            yield break;
        }

        Working = true;

        AssetBundleCreateRequest assetRequest = AssetBundle.LoadFromFileAsync(Path.Combine(assetRootFolder, bundleName));
        if (assetRequest == null)
        {
            Log.Error($"Failed to load AssetBundle: {bundleName}");
            Working = false;
            yield break;
        }
        yield return assetRequest;

        AssetBundleRequest fetchAssetRequest = assetRequest.assetBundle.LoadAssetAsync<GameObject>(bundleName);
        yield return fetchAssetRequest;

        GameObject asset = UnityEngine.Object.Instantiate(fetchAssetRequest.asset, uGUI.main.screenCanvas.transform, false) as GameObject;

        if (!asset)
        {
            Log.Error($"Instantiated assetBundle ({bundleName}) but gameObject is null.");
            Working = false;
            yield break;
        }

        if (hideUI && asset.TryGetComponent(out CanvasGroup canvasGroup))
        {
            canvasGroup.alpha = 0;
        }

        callback.Invoke(asset);
        loadedBundles.Add(bundleName);
        Working = false;
    }

    public static bool HasBundleLoaded(NitroxAssetBundle nitroxAssetBundle)
    {
        return loadedBundles.Contains(nitroxAssetBundle.BundleName);
    }

    public static bool HasBundleLoaded(string bundleName)
    {
        return loadedBundles.Contains(bundleName);
    }

    // Equivalent to an enum with string values
    public class NitroxAssetBundle
    {
        private NitroxAssetBundle(string bundleName) { BundleName = bundleName; }
        public string BundleName { get; private set; }

        public static NitroxAssetBundle SHARED_ASSETS { get { return new NitroxAssetBundle("sharedassets"); } }
        public static NitroxAssetBundle PLAYER_LIST_TAB { get { return new NitroxAssetBundle("playerlisttab"); } }
        public static NitroxAssetBundle CHAT_LOG { get { return new NitroxAssetBundle("chatlog"); } }
        public static NitroxAssetBundle CHAT_KEY_HINT { get { return new NitroxAssetBundle("chatkeyhint"); } }
        public static NitroxAssetBundle DISCORD_JOIN_REQUEST { get { return new NitroxAssetBundle("discordjoinrequest"); } }
    }
}
