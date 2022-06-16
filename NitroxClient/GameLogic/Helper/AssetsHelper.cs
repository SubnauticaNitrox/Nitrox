using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NitroxClient.Unity.Helper.AssetBundleLoader;

namespace NitroxClient.GameLogic.Helper;

public class AssetsHelper
{
    private static Dictionary<string, Texture2D> playerListTabImages = new();
    public static OnPlayerListAssetsLoaded onPlayerListAssetsLoaded;

    public static void Initialize()
    {
        Player.main.StartCoroutine(LoadAssets());
    }

    private static IEnumerator LoadAssets()
    {
        yield return LoadAssetBundle(NitroxAssetBundle.SHARED_ASSETS);
        yield return LoadPlayerListAssets();
    }

    private static IEnumerator LoadPlayerListAssets()
    {
        yield return LoadAssetBundle(NitroxAssetBundle.PLAYER_LIST_TAB, bundle =>
        {
            object[] assets = bundle.LoadAllAssets();
            foreach (object asset in assets)
            {
                if (asset is Texture2D texture)
                {
                    playerListTabImages.Add(texture.name, texture);
                }
            }
            onPlayerListAssetsLoaded?.Invoke();
        });
    }

    public static Texture2D GetTexture(string assetName)
    {
        if (playerListTabImages.TryGetValue(assetName, out Texture2D texture))
        {
            return texture;
        }
        return new Texture2D(100, 100);
    }

    public static Sprite MakeSpriteFromTexture(string assetName)
    {
        Texture2D tex = GetTexture(assetName);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2), 100);
    }

    public static Atlas.Sprite MakeAtlasSpriteFromTexture(string assetName)
    {
        Texture2D tex = GetTexture(assetName);
        return new Atlas.Sprite(tex);
    }

    public delegate void OnPlayerListAssetsLoaded();
}
