using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.FMOD;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FMODUWE_PlayOneShotImpl_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => FMODUWE.PlayOneShotImpl(default(string), default(Vector3), default(float)));

        public static void Prefix(string eventPath, Vector3 position, ref bool __runOriginal)
        {
            if (Resolve<FMODSystem>().TryGetSoundData(eventPath, out SoundData soundData))
            {
                if (Vector3.Distance(Player.main.transform.position, position) >= soundData.SoundRadius)
                {
                    __runOriginal = false;
                    return;
                }
            }
            __runOriginal = true;
        }

        public static void Postfix(string eventPath, Vector3 position, float volume, bool __runOriginal)
        {
            if (!__runOriginal)
            {
                return;
            }

            if (Resolve<FMODSystem>().IsWhitelisted(eventPath, out bool isGlobal, out float radius))
            {
                Resolve<FMODSystem>().PlayAsset(eventPath, position.ToDto(), volume, radius, isGlobal);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, prefix: true, postfix: true);
        }
    }
}
