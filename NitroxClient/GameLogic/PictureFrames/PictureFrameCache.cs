using System.Collections.Concurrent;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PictureFrames;

/// <summary>
/// Cache of image textures keyed by content hash, and the lazy fetch-on-demand orchestration for hashes not yet in the cache.
/// </summary>
public class PictureFrameCache
{
    private readonly IPacketSender packetSender;
    private readonly ConcurrentDictionary<string, Texture2D> texturesByHash = new();
    private readonly ConcurrentDictionary<string, byte> pendingRequests = new();

    public PictureFrameCache(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }
    
    public void Seed(string contentHash, Texture2D texture)
    {
        texturesByHash[contentHash] = texture;
    }

    public bool TryGetTexture(string contentHash, out Texture2D texture) => texturesByHash.TryGetValue(contentHash, out texture);
    
    public void EnsureRequested(NitroxId frameId, string contentHash)
    {
        if (texturesByHash.ContainsKey(contentHash) || !pendingRequests.TryAdd(contentHash, 0))
        {
            return;
        }
        packetSender.Send(new PictureFrameDataRequest(frameId, contentHash));
    }

    public void OnResponseReceived(string contentHash, byte[] jpegBytes, bool found)
    {
        pendingRequests.TryRemove(contentHash, out _);
        if (!found || jpegBytes == null || jpegBytes.Length == 0)
        {
            return;
        }

        Texture2D texture = new(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(jpegBytes, false))
        {
            UnityEngine.Object.Destroy(texture);
            return;
        }

        texturesByHash[contentHash] = texture;
    }
}
