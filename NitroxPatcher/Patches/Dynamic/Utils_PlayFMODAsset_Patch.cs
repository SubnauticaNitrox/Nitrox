using System.Reflection;
using NitroxClient.GameLogic.FMOD;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Utils_PlayFMODAsset_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => Utils.PlayFMODAsset(default(FMODAsset)));

    public static bool Prefix()
    {
        return !FMODSoundSuppressor.SuppressFMODEvents;
    }

    public static void Postfix(FMODAsset asset)
    {
        if (Resolve<FMODWhitelist>().IsWhitelisted(asset.path))
        {
            Resolve<FMODSystem>().SendAssetPlay(asset.path, Player.main.transform.position.ToDto(), 1f);
        }
    }
}
