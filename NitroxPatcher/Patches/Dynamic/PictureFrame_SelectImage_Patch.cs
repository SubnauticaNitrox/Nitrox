using System.IO;
using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Server;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PictureFrames;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Fires when the local player commits a picture selection from the gallery (<see cref="PictureFrame.SelectImage" />).
/// Will process the image selection locally and upload it to the server if picture frame syncing is enabled.
/// </summary>

public sealed partial class PictureFrame_SelectImage_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PictureFrame t) => t.SelectImage(null));

    public static bool Prefix(PictureFrame __instance, string image)
    {
        // Use vanilla's own fileName/SetState
        if (PacketSuppressor<EntityMetadataUpdate>.IsSuppressed)
        {
            return true;
        }

        LocalPlayer localPlayer = Resolve<LocalPlayer>();
        if (localPlayer.PictureFrameSync == PictureFrameSyncMode.OFF)
        {
            return true;
        }

        if (!__instance.gameObject.FindAncestor<PrefabIdentifier>().TryGetIdOrWarn(out NitroxId id))
        {
            return false;
        }

        if (string.IsNullOrEmpty(image))
        {
            ApplyLocally(__instance, null);
            Resolve<Entities>().BroadcastMetadataUpdate(id, new PictureFrameMetadata(null));
            return false;
        }

        string filePath = Path.Combine(NitroxDirectory.ScreenshotsPath, Path.GetFileName(image));
        PictureFrameContentBuilder.Result result = PictureFrameContentBuilder.TryBuild(
            filePath,
            localPlayer.PictureFrameMaxDimension,
            localPlayer.PictureFrameJpegQuality,
            localPlayer.PictureFrameMaxBytes);

        if (!result.Success)
        {
            Log.Warn($"Rejected picture frame selection '{image}': {result.ErrorMessage}");
            ErrorMessage.AddMessage(result.ErrorMessage);
            return false;
        }

        Resolve<PictureFrameCache>().Seed(result.ContentHash, result.Texture);
        Resolve<IPacketSender>().Send(new PictureFrameDataUpload(id, result.ContentHash, result.JpegBytes));
        Resolve<Entities>().BroadcastMetadataUpdate(id, new PictureFrameMetadata(result.ContentHash));
        ApplyLocally(__instance, result.ContentHash);
        return false;
    }
    
    private static void ApplyLocally(PictureFrame pictureFrame, string contentHash)
    {
        if (string.Equals(pictureFrame.fileName, contentHash))
        {
            return;
        }

        pictureFrame.SetState(PictureFrame.State.None);
        pictureFrame.fileName = contentHash;
        pictureFrame.SetState(pictureFrame.desired);
    }
}
