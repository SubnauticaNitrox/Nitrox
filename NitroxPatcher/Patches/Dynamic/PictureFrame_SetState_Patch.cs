using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PictureFrames;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// We wish to bypass the screenshot manager entirely and render from <see cref="PictureFrameCache" /> instead, fetching over the network on a cache miss.
/// This is a no-op when syncing is off
/// </summary>
public sealed partial class PictureFrame_SetState_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PictureFrame t) => t.SetState(default));

    public static bool Prefix(PictureFrame __instance, PictureFrame.State newState)
    {
        if (!Resolve<LocalPlayer>().PictureFrameSyncActive)
        {
            return true;
        }

        if (__instance.current == newState)
        {
            return false;
        }

        __instance.SetTexture(null);
        __instance.current = PictureFrame.State.None;

        if (newState != PictureFrame.State.None && !string.IsNullOrEmpty(__instance.fileName))
        {
            string contentHash = __instance.fileName;
            PictureFrameCache cache = Resolve<PictureFrameCache>();
            if (cache.TryGetTexture(contentHash, out Texture2D texture))
            {
                __instance.SetTexture(texture);
                __instance.current = PictureFrame.State.Full;
            }
            else if (__instance.gameObject.FindAncestor<PrefabIdentifier>().TryGetIdOrWarn(out NitroxId id))
            {
                cache.EnsureRequested(id, contentHash);
            }
        }

        return false;
    }
}
