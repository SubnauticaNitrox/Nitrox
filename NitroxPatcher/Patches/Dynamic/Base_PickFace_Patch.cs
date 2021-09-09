using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata;

namespace NitroxPatcher.Patches.Dynamic
{
    class Base_PickFace_Patch : NitroxPatch, IDynamicPatch
    {
        public readonly MethodInfo METHOD = typeof(Base).GetMethod(nameof(Base.PickFace), BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(Base __instance, ref bool __result, ref Base.Face face)
        {
            if(MultiplayerBuilder.rotationMetadata.HasValue)
            {
                switch (MultiplayerBuilder.rotationMetadata.Value)
                {
                    case AnchoredFaceRotationMetadata anchoredFaceRotationMetadata:
                        face = new Base.Face(anchoredFaceRotationMetadata.Cell.ToUnity(), (Base.Direction)anchoredFaceRotationMetadata.Direction);
                        __result = true;
                        return false;
                    case BaseModuleRotationMetadata baseModuleRotationMetadata:
                        face = new Base.Face(baseModuleRotationMetadata.Cell.ToUnity(), (Base.Direction)baseModuleRotationMetadata.Direction);
                        __result = true;
                        return false;
                }
            }
            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, METHOD);
        }
    }
}
