using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap;

/// <summary>
/// Tracks the RenderTextures a player's model was recolored into by <see cref="GpuRecolorer"/> so they get
/// released when the model is destroyed. Unlike Texture2D, RenderTextures are a scarce GPU resource that
/// Unity's GC does not reclaim on its own - forgetting to Release() one leaks VRAM every join/leave cycle.
/// Attached lazily to the player model root; <see cref="MonoBehaviour.OnDestroy"/> firing when
/// <see cref="RemotePlayer.Destroy"/> calls <c>Object.DestroyImmediate(Body)</c> is what triggers cleanup, so
/// there's no separate teardown call site to remember.
/// </summary>
public class PlayerColorRenderTextures : MonoBehaviour
{
    private readonly List<RenderTexture> renderTextures = new();

    public static PlayerColorRenderTextures GetOrAdd(GameObject playerModel)
    {
        return playerModel.TryGetComponent(out PlayerColorRenderTextures existing) ? existing : playerModel.AddComponent<PlayerColorRenderTextures>();
    }

    public void Track(RenderTexture renderTexture)
    {
        renderTextures.Add(renderTexture);
    }

    private void OnDestroy()
    {
        foreach (RenderTexture renderTexture in renderTextures)
        {
            renderTexture.Release();
        }
        renderTextures.Clear();
    }
}
