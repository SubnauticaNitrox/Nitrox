using System;
using System.Collections;
using System.IO;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.Unity.Helper;

public static class AssetBundleLoader
{
    private static readonly string assetRootFolder = NitroxUser.AssetsPath;

    private static bool loadedSharedAssets;

    public static IEnumerator LoadAssetBundle(NitroxAssetBundle nitroxAssetBundle)
    {
        if (IsBundleLoaded(nitroxAssetBundle))
        {
            yield break;
        }
        if (!loadedSharedAssets)
        {
            loadedSharedAssets = true;
            yield return LoadAssetBundle(NitroxAssetBundle.SHARED_ASSETS);
        }

        AssetBundleCreateRequest assetRequest = AssetBundle.LoadFromFileAsync(Path.Combine(assetRootFolder, nitroxAssetBundle.BundleName));
        if (assetRequest == null)
        {
            Log.Error($"Failed to load AssetBundle: {nitroxAssetBundle.BundleName}");
            yield break;
        }
        
        yield return assetRequest;
        AssetBundleRequest loadRequest = assetRequest.assetBundle.LoadAllAssetsAsync();
        yield return loadRequest;
        

        if (loadRequest.allAssets == null || loadRequest.allAssets.Length == 0)
        {
            Log.Error($"Failed to load AssetBundle: {nitroxAssetBundle.BundleName}. It contained no assets");
            yield break;
        }

        nitroxAssetBundle.LoadedAssets = loadRequest.allAssets;
    }

    public static IEnumerator LoadUIAsset(NitroxAssetBundle nitroxAssetBundle, bool hideUI)
    {
        if (IsBundleLoaded(nitroxAssetBundle))
        {
            yield break;
        }
        if (!loadedSharedAssets)
        {
            loadedSharedAssets = true;
            yield return LoadAssetBundle(NitroxAssetBundle.SHARED_ASSETS);
        }

        AssetBundleCreateRequest assetRequest = AssetBundle.LoadFromFileAsync(Path.Combine(assetRootFolder, nitroxAssetBundle.BundleName));
        if (assetRequest == null)
        {
            Log.Error($"Failed to load AssetBundle: {nitroxAssetBundle.BundleName}");
            yield break;
        }
        yield return assetRequest;

        AssetBundleRequest fetchAssetRequest = assetRequest.assetBundle.LoadAssetAsync<GameObject>(nitroxAssetBundle.BundleName);
        yield return fetchAssetRequest;

        GameObject asset = UnityEngine.Object.Instantiate(fetchAssetRequest.asset, uGUI.main.screenCanvas.transform, false) as GameObject;

        if (!asset)
        {
            Log.Error($"Instantiated assetBundle ({nitroxAssetBundle.BundleName}) but GameObject is null.");
            yield break;
        }

        if (hideUI && asset.TryGetComponent(out CanvasGroup canvasGroup))
        {
            canvasGroup.alpha = 0;
        }
        nitroxAssetBundle.LoadedAssets = new UnityEngine.Object[] { asset };
    }

    public static bool IsBundleLoaded(NitroxAssetBundle nitroxAssetBundle)
    {
        return nitroxAssetBundle.LoadedAssets != null && nitroxAssetBundle.LoadedAssets.Length > 0;
    }

    // ReSharper disable class StringLiteralTypo, InconsistentNaming
    public class NitroxAssetBundle
    {
        public string BundleName { get; }
        public UnityEngine.Object[] LoadedAssets { get; set; }

        private NitroxAssetBundle(string bundleName)
        {
            BundleName = bundleName;
        }

        public static readonly NitroxAssetBundle SHARED_ASSETS = new("sharedassets");
        public static readonly NitroxAssetBundle PLAYER_LIST_TAB =  new("playerlisttab");
        public static readonly NitroxAssetBundle CHAT_LOG =  new("chatlog");
        public static readonly NitroxAssetBundle CHAT_KEY_HINT =  new("chatkeyhint");
        public static readonly NitroxAssetBundle DISCORD_JOIN_REQUEST = new("discordjoinrequest");
    }
}
