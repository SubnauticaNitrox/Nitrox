using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel.Logger;
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
            if(MultiplayerBuilder.rotationMetadata.HasValue)
            {
                switch (MultiplayerBuilder.rotationMetadata.Value)
                {
                    case AnchoredFaceRotationMetadata anchoredFaceRotationMetadata:
                        __result = new Base.Face(anchoredFaceRotationMetadata.Cell.ToUnity(), (Base.Direction)anchoredFaceRotationMetadata.Direction);
                        Log.Info($"Base.PickFace: {face.cell} {face.direction}");
                        return false;
                    case BaseModuleRotationMetadata baseModuleRotationMetadata:
                        __result = new Base.Face(baseModuleRotationMetadata.Cell.ToUnity(), (Base.Direction)baseModuleRotationMetadata.Direction);
                        Log.Info($"Base.PickFace: {face.cell} {face.direction}");
                        return false;
                }
                Log.Info($"rotationMetadata type not found.");
            }
            else
            {
                Log.Info($"rotationMetadata no value");
            }
            
            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, METHOD);
        }
    }
}
