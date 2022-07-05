using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata;

namespace NitroxPatcher.Patches.Dynamic
{
    class Base_GetAdjacentFace_Patch : NitroxPatch, IDynamicPatch
    {
        public readonly MethodInfo METHOD = typeof(Base).GetMethod(nameof(Base.GetAdjacentFace), BindingFlags.Public | BindingFlags.Static);

        public static bool Prefix(ref Base.Face __result, Base.Face face)
        {
            if(MultiplayerBuilder.RotationMetadata.HasValue)
            {
                switch (MultiplayerBuilder.RotationMetadata.Value)
                {
                    case AnchoredFaceBuilderMetadata anchoredFaceRotationMetadata:
                        __result = new Base.Face(anchoredFaceRotationMetadata.Cell.ToUnity(), (Base.Direction)anchoredFaceRotationMetadata.Direction);
                        return false;
                    case BaseModuleBuilderMetadata baseModuleRotationMetadata:
                        __result = new Base.Face(baseModuleRotationMetadata.Cell.ToUnity(), (Base.Direction)baseModuleRotationMetadata.Direction);
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
