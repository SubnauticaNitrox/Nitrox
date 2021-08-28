using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxModel.Core;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Utils_PlayFMODAsset_Patch : NitroxPatch, IDynamicPatch
    {
        private static FMODSystem fmodSystem;

        private static readonly MethodInfo targetMethod = typeof(Utils).GetMethod(nameof(Utils.PlayFMODAsset), BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(FMODAsset) }, null);

        public static bool Prefix()
        {
            return !FMODSuppressor.SuppressFMODEvents;
        }

        public static void Postfix(FMODAsset asset)
        {
            if (fmodSystem.IsWhitelisted(asset.path, out bool isGlobal, out float radius))
            {
                fmodSystem.PlayAsset(asset.path, Player.main.transform.position.ToDto(), 1f, radius, isGlobal);
            }
        }

        public override void Patch(Harmony harmony)
        {
            fmodSystem = NitroxServiceLocator.LocateService<FMODSystem>();
            PatchMultiple(harmony, targetMethod, prefix:true, postfix:true);
        }
    }
}
