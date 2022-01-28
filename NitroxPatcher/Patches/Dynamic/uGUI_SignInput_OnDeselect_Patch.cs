using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class uGUI_SignInput_OnDeselect_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_SignInput t) => t.OnDeselect());

        public static void Postfix(uGUI_SignInput __instance)
        {
            GameObject gameObject = __instance.gameObject.FindAncestor<PrefabIdentifier>().gameObject;
            NitroxId id = NitroxEntity.GetId(gameObject);
            TechTag tag = gameObject.GetComponent<TechTag>();
            
            switch (tag.type)
            {
                case TechType.SmallStorage:
                    // In the water
                    EntitySignMetadata entitySignMetadata = new(__instance.text, __instance.colorIndex, __instance.scaleIndex, __instance.elementsState, __instance.IsBackground());
                    Resolve<Entities>().BroadcastMetadataUpdate(id, entitySignMetadata);
                    break;
                case TechType.Sign:
                case TechType.SmallLocker:
                    // On wall
                    SignMetadata signMetadata = new(__instance.text, __instance.colorIndex, __instance.scaleIndex, __instance.elementsState, __instance.IsBackground());
                    Resolve<Building>().MetadataChanged(id, signMetadata);
                    break;
                default:
                    Log.Warn($"[{nameof(uGUI_SignInput_OnDeselect_Patch)}] no case planned for tech type {tag.type}");
                    break;
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
