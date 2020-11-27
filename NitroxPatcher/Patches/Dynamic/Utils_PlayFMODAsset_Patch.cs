using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Utils_PlayFMODAsset_Patch : NitroxPatch, IDynamicPatch
    {
        private static FMODSystem fmodSystem;
        private static readonly MethodInfo targetMethod = typeof(Utils).GetMethod(nameof(Utils.PlayFMODAsset), BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(FMODAsset), typeof(GameObject), typeof(Vector3), typeof(float) }, null);

        public static void Postfix(FMODAsset asset, Vector3 position, float soundRadiusObsolete)
        {
            Validate.NotNull(asset);
            if (fmodSystem.IsWhitelisted(asset.path, out bool isGlobal))
            {
                fmodSystem.PlayFMODAsset(asset.path, position.ToDto(), soundRadiusObsolete, isGlobal);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            fmodSystem = NitroxServiceLocator.LocateService<FMODSystem>();
            PatchPostfix(harmony, targetMethod);
        }
    }
}
