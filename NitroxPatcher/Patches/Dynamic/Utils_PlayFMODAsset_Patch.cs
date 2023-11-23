using System.Reflection;
using NitroxClient.GameLogic.FMOD;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Utils_PlayFMODAsset_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => Utils.PlayFMODAsset(default(FMODAsset)));

    public static bool Prefix()
    {
        return !FMODSuppressor.SuppressFMODEvents;
    }

    public static void Postfix(FMODAsset asset)
    {
        if (Resolve<FMODSystem>().IsWhitelisted(asset.path, out bool isGlobal, out float radius))
        {
            Resolve<FMODSystem>().PlayAsset(asset.path, Player.main.transform.position.ToDto(), 1f, radius, isGlobal);
        }
    }
}
