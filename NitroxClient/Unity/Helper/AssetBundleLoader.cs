using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.Unity.Helper;

public static class AssetBundleLoader
{
    internal static readonly string assetRootFolder = NitroxUser.AssetsPath;

    private readonly static Dictionary<string, LoadedAssetBundle> loadedBundles = new();
    private readonly static Dictionary<string, AssetBundleLoadedEvent> subscribedEvents = new() { { "*", null } };
    public static Dictionary<string, Atlas.Sprite> PDATabSprites = new();
    private static bool loadedSharedAssets = false;

    public static IEnumerator LoadAssetBundle(string bundleName, Action<LoadedAssetBundle> callback = null)
    {
        if (IsBundleLoaded(bundleName))
        {
            yield break;
        }
        if (!loadedSharedAssets)
        {
            loadedSharedAssets = true;
            yield return LoadAssetBundle(NitroxAssetBundle.SHARED_ASSETS);
        }

        AssetBundleCreateRequest assetRequest = AssetBundle.LoadFromFileAsync(Path.Combine(assetRootFolder, bundleName));
        if (assetRequest == null)
        {
            Log.Error($"Failed to load AssetBundle: {bundleName}");
            yield break;
        }
        
        yield return assetRequest;
        AssetBundleRequest loadRequest = assetRequest.assetBundle.LoadAllAssetsAsync();
        yield return loadRequest;

        loadedBundles[bundleName] = new(assetRequest.assetBundle, loadRequest.allAssets.ToList());
        callback?.Invoke(loadedBundles[bundleName]);

        subscribedEvents.TryGetValue(bundleName, out AssetBundleLoadedEvent assetBundleLoadedEvent);
        assetBundleLoadedEvent?.Invoke();
        subscribedEvents["*"]?.Invoke();
    }

    public static IEnumerator LoadUIAsset(string bundleName, bool hideUI, Action<GameObject> callback)
    {
        if (IsBundleLoaded(bundleName))
        {
            yield break;
        }
        if (!loadedSharedAssets)
        {
            loadedSharedAssets = true;
            yield return LoadAssetBundle(NitroxAssetBundle.SHARED_ASSETS);
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
        
        loadedBundles[bundleName] = new (assetRequest.assetBundle, new() { asset });
        callback.Invoke(asset);

        subscribedEvents.TryGetValue(bundleName, out AssetBundleLoadedEvent assetBundleLoadedEvent);
        assetBundleLoadedEvent?.Invoke();
        subscribedEvents["*"]?.Invoke();
    }

    public static bool IsBundleLoaded(string bundleName)
    {
        return loadedBundles.ContainsKey(bundleName);
    }
    
    public static bool TryGetAssetBundle(string bundleName, out LoadedAssetBundle assetBundle)
    {
        loadedBundles.TryGetValue(bundleName, out assetBundle);
        return assetBundle != null;
    }

    public static void SubscribeToEvent(string bundleName, AssetBundleLoadedEvent callback)
    {
        if (!subscribedEvents.TryGetValue(bundleName, out AssetBundleLoadedEvent assetBundleLoadedEvent))
        {
            subscribedEvents[bundleName] = callback;
            return;
        }
        subscribedEvents[bundleName] += callback;
    }

    public delegate void AssetBundleLoadedEvent();

    public class LoadedAssetBundle
    {
        public AssetBundle AssetBundle;
        public List<UnityEngine.Object> AllAssets;

        public LoadedAssetBundle(AssetBundle assetBundle, List<UnityEngine.Object> allAssets)
        {
            AssetBundle = assetBundle;
            AllAssets = allAssets;
        }
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

        public static implicit operator string(NitroxAssetBundle nitroxAssetBundle)
        {
            return nitroxAssetBundle.BundleName;
        }
    }
}
