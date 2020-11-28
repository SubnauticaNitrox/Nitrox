using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FMODUWE_PlayOneShotImpl_Patch : NitroxPatch, IDynamicPatch
    {
        private static FMODSystem fmodSystem;

        private static readonly MethodInfo targetMethod = typeof(FMODUWE).GetMethod("PlayOneShotImpl", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(string), typeof(Vector3), typeof(float) }, null);

        public static void Postfix(string eventPath, Vector3 position, float volume)
        {
            if (fmodSystem.IsWhitelisted(eventPath, out bool isGlobal, out float radius))
            {
                fmodSystem.PlayFMODAsset(eventPath, position.ToDto(), volume, radius, isGlobal);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            fmodSystem = NitroxServiceLocator.LocateService<FMODSystem>();
            PatchPostfix(harmony, targetMethod);
        }
    }
}
